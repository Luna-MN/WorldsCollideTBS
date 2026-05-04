package server

import (
	"context"
	"database/sql"
	_ "embed"
	"log"
	"net/http"
	"server/internal/server/db"
	"server/internal/server/objects"
	"server/pkg/packets"

	_ "modernc.org/sqlite"
)

//go:embed db/config/schema.sql
var schemaGenSql string

// DbTx A structure for database transaction context
type DbTx struct {
	Ctx     context.Context
	Queries *db.Queries
}

func (h *Hub) NewDbTx() *DbTx {
	return &DbTx{
		Ctx:     context.Background(),
		Queries: db.New(h.dbPool),
	}
}

type SharedGameObjects struct {
	// ID of the player is the ID of the client
	Players *objects.SharedCollection[*objects.Player]
}

// ClientStateHandler A structure for a state macgine to process the client's messages
type ClientStateHandler interface {
	Name() string

	// SetClient Inject thje client into the state machine
	SetClient(client *Client)
	CanReceiveGlobalChat() bool
	OnEnter()
	HandleMessage(senderId uint64, message packets.Msg, transfer TransferType)
	// OnExit clean up the state machine
	OnExit()
}

type ClientInterfacer interface {
	// Initialize sets the client's Id and anything else to be initialized
	Initialize(id uint64)
	SetState(newState ClientStateHandler, sync ...bool)
	Id() uint64
	IsEnet() bool
	SetClient(client *Client)
	Client() *Client
	ProcessMessage(senderId uint64, msg packets.Msg, enetChannel ...int)

	// SocketSend Puts data from this client into the write pump
	SocketSend(message packets.Msg, enetChannel ...int)

	// SocketSendAs puts data from another client into the write pump
	SocketSendAs(message packets.Msg, senderId uint64, enetChannel ...int)

	// PassToPeer forward message to another client for processing
	PassToPeer(message packets.Msg, peerId uint64)

	// Broadcast send message to all clients except the sender
	Broadcast(message packets.Msg, enet bool)

	// ReadPump Pump data from the connected socket directly to the client
	ReadPump()

	// WritePump Pump data from the client directly to the connected socket
	WritePump()

	// DbTx A reference to the database transation context for this client
	DbTx() *DbTx

	SharedGameObjects() *SharedGameObjects

	// Close closes the connection
	Close(reason string)
}
type BroadcastItem struct {
	Packet *packets.Packet
	State  string
	ToAll  bool
	Enet   bool
}

// Hub is the central point of communication for the server, managing connections and broadcasting messages to clients.
type Hub struct {
	Clients *objects.SharedCollection[*Client]
	//Clients *objects.SharedCollection[ClientInterfacer]
	PeerToClient map[uint]*Client

	// Packets in this channel will be processed by all connected clients except the sender
	BroadcastChan chan *BroadcastItem

	// Clients that want to register
	RegisterChan chan ClientInterfacer

	// Clients that want to unregister
	UnregisterChan chan ClientInterfacer

	dbPool *sql.DB

	SharedGameObjects *SharedGameObjects

	PacketCounter uint64
}

func NewHub() *Hub {
	dbPool, err := sql.Open("sqlite", "db.sqlite")
	if err != nil {
		log.Fatal(err)
	}
	return &Hub{
		Clients:        objects.NewSharedCollection[*Client](),
		PeerToClient:   make(map[uint]*Client),
		BroadcastChan:  make(chan *BroadcastItem),
		RegisterChan:   make(chan ClientInterfacer),
		UnregisterChan: make(chan ClientInterfacer),
		dbPool:         dbPool,
		SharedGameObjects: &SharedGameObjects{
			Players: objects.NewSharedCollection[*objects.Player](),
		},
	}
}

func (h *Hub) Run() {
	log.Println("Initializing database")
	_, err := h.dbPool.ExecContext(context.Background(), schemaGenSql)
	if err != nil {
		log.Fatal(err)
	}
	log.Println("Awaiting clients registration")
	for {
		select {
		case client := <-h.RegisterChan:
			var clients = NewClient(client, h)
			var Id = h.Clients.Add(clients)
			clients.Initialize(Id)
			client.Initialize(Id)
		case client := <-h.UnregisterChan:
			_, b := h.Clients.Get(client.Id())
			if b == true {
				h.Clients.Remove(client.Id())
			}
		case packet := <-h.BroadcastChan:
			//for id, client := range h.Clients {
			transType := WebSocket
			if packet.Enet {
				transType = ENet_Channel0
			}
			h.Clients.ForEach(func(id uint64, client *Client) {
				if id != packet.Packet.SenderId {
					//blocks traffic to clients in a differant state
					if !packet.ToAll && client.state.Name() != packet.State {
						return
					}
					client.ProcessMessage(packet.Packet.SenderId, packet.Packet.Msg, transType)
				}
			})
		}
	}
}

func (h *Hub) Serve(getNewClient func(*Hub, http.ResponseWriter, *http.Request) (ClientInterfacer, error), writer http.ResponseWriter, request *http.Request) {
	log.Println("New client connected from", request.RemoteAddr)
	client, err := getNewClient(h, writer, request)

	if err != nil {
		log.Printf("Error obtaining client for new connection: %v", err)
		return
	}

	h.RegisterChan <- client

	go client.WritePump()
	go client.ReadPump()
}
