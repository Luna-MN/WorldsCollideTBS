package clients

import (
	"fmt"
	"log"
	"net/http"
	"server/internal/server"
	"server/internal/server/states"
	"server/pkg/packets"

	"github.com/gorilla/websocket"
	"google.golang.org/protobuf/proto"
)

type WebSocketClient struct {
	id       uint64
	conn     *websocket.Conn
	hub      *server.Hub
	sendChan chan *packets.Packet
	logger   *log.Logger
	dbTx     *server.DbTx
	client   *server.Client
}

func (c *WebSocketClient) Client() *server.Client {
	return c.client
}

func (c *WebSocketClient) SetClient(client *server.Client) {
	c.client = client
}

func (c *WebSocketClient) IsEnet() bool {
	return false
}

func (c *WebSocketClient) SetState(newState server.ClientStateHandler, sync ...bool) {
	c.client.SetState(newState, sync...)
}

func NewWebSocketClient(hub *server.Hub, writer http.ResponseWriter, request *http.Request) (server.ClientInterfacer, error) {
	upgrader := websocket.Upgrader{
		ReadBufferSize:  1024,
		WriteBufferSize: 1024,
		CheckOrigin:     func(_ *http.Request) bool { return true },
	}

	conn, err := upgrader.Upgrade(writer, request, nil)

	if err != nil {
		return nil, err
	}

	c := &WebSocketClient{
		hub:      hub,
		conn:     conn,
		sendChan: make(chan *packets.Packet, 256),
		logger:   log.New(log.Writer(), "Client unknown: ", log.LstdFlags),
		dbTx:     hub.NewDbTx(),
	}

	return c, nil
}

func (c *WebSocketClient) Id() uint64 {
	return c.id
}

func (c *WebSocketClient) ProcessMessage(senderId uint64, message packets.Msg, enetChannel ...int) {
	c.client.ProcessMessage(senderId, message, server.WebSocket)
}

func (c *WebSocketClient) Initialize(id uint64) {
	c.id = id
	c.logger.SetPrefix(fmt.Sprintf("Client %d: ", c.id))
	c.SetState(&states.Login{})
}

func (c *WebSocketClient) SocketSend(message packets.Msg, enetChannel ...int) {
	c.SocketSendAs(message, c.id)
}

func (c *WebSocketClient) SocketSendAs(message packets.Msg, senderId uint64, enetChannel ...int) {
	select {
	case c.sendChan <- &packets.Packet{SenderId: senderId, Msg: message}:
	default:
		c.logger.Printf("Send channel full, dropping message: %T", message)
	}
}

func (c *WebSocketClient) PassToPeer(message packets.Msg, peerId uint64) {
	if peer, exists := c.hub.Clients.Get(peerId); exists {
		peer.WS.ProcessMessage(c.id, message)
	}
}

func (c *WebSocketClient) Broadcast(message packets.Msg, enet bool) {
	c.hub.BroadcastChan <- &server.BroadcastItem{Packet: &packets.Packet{SenderId: c.id, Msg: message}, Enet: enet}
}

func (c *WebSocketClient) ReadPump() {
	defer func() {
		c.logger.Println("Closing read pump")
		c.Close("read pump closed")
	}()

	for {
		_, data, err := c.conn.ReadMessage()
		if err != nil {
			c.logger.Printf("ReadMessage error: %T: %v", err, err)

			if websocket.IsCloseError(err,
				websocket.CloseNormalClosure,
				websocket.CloseGoingAway,
				websocket.CloseAbnormalClosure,
			) {
				c.logger.Printf("WebSocket closed (one of: normal/going away/abnormal)")
			}

			if ce, ok := err.(*websocket.CloseError); ok {
				c.logger.Printf("WebSocket close details: code=%d text=%q", ce.Code, ce.Text)
			}
			break
		}

		packet := &packets.Packet{}
		err = proto.Unmarshal(data, packet)
		if err != nil {
			c.logger.Printf("error unmarshalling data: %v", err)
			continue
		}

		// To allow the client to lazily not send the sender ID, we'll assume they want to send it as themselves
		if packet.SenderId == 0 {
			packet.SenderId = c.id
		}

		c.ProcessMessage(packet.SenderId, packet.Msg)
	}
}

func (c *WebSocketClient) WritePump() {
	defer func() {
		c.logger.Println("Closing write pump")
		c.Close("write pump closed")
	}()

	for packet := range c.sendChan {
		writer, err := c.conn.NextWriter(websocket.BinaryMessage)
		if err != nil {
			c.logger.Printf("error getting writer for %T packet, closing client: %v", packet.Msg, err)
			return
		}

		data, err := proto.Marshal(packet)
		if err != nil {
			c.logger.Printf("error marshalling %T packet, closing client: %v", packet.Msg, err)
			continue
		}

		_, err = writer.Write(data)
		if err != nil {
			c.logger.Printf("error writing %T packet: %v", packet.Msg, err)
			continue
		}

		if err = writer.Close(); err != nil {
			c.logger.Printf("error closing writer for %T packet: %v", packet.Msg, err)
			continue
		}
	}
}

func (c *WebSocketClient) DbTx() *server.DbTx {
	return c.dbTx
}

func (c *WebSocketClient) SharedGameObjects() *server.SharedGameObjects {
	return c.hub.SharedGameObjects
}

func (c *WebSocketClient) Close(reason string) {
	c.logger.Printf("Closing client connection because: %s", reason)

	c.SetState(nil)

	c.hub.UnregisterChan <- c
	err := c.conn.Close()
	if err != nil {
		return
	}
	if _, closed := <-c.sendChan; !closed {
		close(c.sendChan)
	}
}
