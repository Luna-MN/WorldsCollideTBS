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

func (w *WebSocketClient) Client() *server.Client {
	return w.client
}

func (w *WebSocketClient) SetClient(client *server.Client) {
	w.client = client
}

func (w *WebSocketClient) IsEnet() bool {
	return false
}

func (w *WebSocketClient) SetState(newState server.ClientStateHandler, sync ...bool) {
	w.client.SetState(newState, sync...)
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

func (w *WebSocketClient) Id() uint64 {
	return w.id
}

func (w *WebSocketClient) ProcessMessage(senderId uint64, message packets.Msg, enetChannel ...int) {
	switch message := message.(type) {
	case *packets.Packet_ChangeState:
		b := w.HandleStateChangeMessage(message.ChangeState.StateName, senderId)
		if b {
			return
		}
	}
	w.client.ProcessMessage(senderId, message, server.WebSocket)
}
func (w *WebSocketClient) HandleStateChangeMessage(state string, id uint64) bool {
	if id != w.id {
		return false
	}
	if state == "Lobby" {
		w.SetState(&states.Lobby{})
		return true
	}
	return false
}
func (w *WebSocketClient) Initialize(id uint64) {
	w.id = id
	w.logger.SetPrefix(fmt.Sprintf("Client %d: ", w.id))
	w.SetState(&states.Login{})
}

func (w *WebSocketClient) SocketSend(message packets.Msg, enetChannel ...int) {
	w.SocketSendAs(message, w.id)
}

func (w *WebSocketClient) SocketSendAs(message packets.Msg, senderId uint64, enetChannel ...int) {
	select {
	case w.sendChan <- &packets.Packet{SenderId: senderId, Msg: message}:
	default:
		w.logger.Printf("Send channel full, dropping message: %T", message)
	}
}

func (w *WebSocketClient) PassToPeer(message packets.Msg, peerId uint64) {
	if peer, exists := w.hub.Clients.Get(peerId); exists {
		peer.WS.ProcessMessage(w.id, message)
	}
}

func (w *WebSocketClient) Broadcast(message packets.Msg, enet bool) {
	w.hub.BroadcastChan <- &server.BroadcastItem{Packet: &packets.Packet{SenderId: w.id, Msg: message}, Enet: enet}
}

func (w *WebSocketClient) ReadPump() {
	defer func() {
		w.logger.Println("Closing read pump")
		w.Close("read pump closed")
	}()

	for {
		_, data, err := w.conn.ReadMessage()
		if err != nil {
			w.logger.Printf("ReadMessage error: %T: %v", err, err)

			if websocket.IsCloseError(err,
				websocket.CloseNormalClosure,
				websocket.CloseGoingAway,
				websocket.CloseAbnormalClosure,
			) {
				w.logger.Printf("WebSocket closed (one of: normal/going away/abnormal)")
			}

			if ce, ok := err.(*websocket.CloseError); ok {
				w.logger.Printf("WebSocket close details: code=%d text=%q", ce.Code, ce.Text)
			}
			break
		}

		packet := &packets.Packet{}
		err = proto.Unmarshal(data, packet)
		if err != nil {
			w.logger.Printf("error unmarshalling data: %v", err)
			continue
		}

		// To allow the client to lazily not send the sender ID, we'll assume they want to send it as themselves
		if packet.SenderId == 0 {
			packet.SenderId = w.id
		}

		w.ProcessMessage(packet.SenderId, packet.Msg)
	}
}

func (w *WebSocketClient) WritePump() {
	defer func() {
		w.logger.Println("Closing write pump")
		w.Close("write pump closed")
	}()

	for packet := range w.sendChan {
		writer, err := w.conn.NextWriter(websocket.BinaryMessage)
		if err != nil {
			w.logger.Printf("error getting writer for %T packet, closing client: %v", packet.Msg, err)
			return
		}

		data, err := proto.Marshal(packet)
		if err != nil {
			w.logger.Printf("error marshalling %T packet, closing client: %v", packet.Msg, err)
			continue
		}

		_, err = writer.Write(data)
		if err != nil {
			w.logger.Printf("error writing %T packet: %v", packet.Msg, err)
			continue
		}

		if err = writer.Close(); err != nil {
			w.logger.Printf("error closing writer for %T packet: %v", packet.Msg, err)
			continue
		}
	}
}

func (w *WebSocketClient) DbTx() *server.DbTx {
	return w.dbTx
}

func (w *WebSocketClient) SharedGameObjects() *server.SharedGameObjects {
	return w.hub.SharedGameObjects
}

func (w *WebSocketClient) Close(reason string) {
	w.logger.Printf("Closing client connection because: %s", reason)

	w.SetState(nil)

	w.hub.UnregisterChan <- w
	err := w.conn.Close()
	if err != nil {
		return
	}
	if _, closed := <-w.sendChan; !closed {
		close(w.sendChan)
	}
}
