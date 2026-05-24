package server

import (
	"crypto/cipher"
	"fmt"
	"log"
	"os"
	"server/internal/server/db"
	"server/internal/server/objects"
	"server/internal/steam"
	"server/pkg/packets"
)

type TransferType int

const (
	WebSocket TransferType = iota
	ENet_Channel0
	ENet_Channel1
)

type Client struct {
	id            uint64
	username      string
	SteamID       string
	IsSteamClient bool
	ENet          ClientInterfacer
	WS            ClientInterfacer
	state         ClientStateHandler
	hub           *Hub
	logger        *log.Logger
	dbTx          *DbTx
	broadcastChan chan packets.Msg
	player        *objects.Player
	Cyph          cipher.AEAD
	Steam         *steam.SteamWebClient
}

func NewClient(WS ClientInterfacer, h *Hub) *Client {
	steamAPIKey, exists := os.LookupEnv("STEAM_API_KEY")
	if !exists {
		fmt.Println("STEAM_API_KEY not set")
	}
	c := &Client{
		id:            0,
		ENet:          nil,
		WS:            WS,
		dbTx:          WS.DbTx(),
		logger:        log.New(log.Writer(), "Undefined Client: ", log.LstdFlags),
		hub:           h,
		broadcastChan: make(chan packets.Msg, 256),
		Steam:         steam.NewClient(steamAPIKey),
	}
	WS.SetClient(c)
	return c
}
func (c *Client) Initialize(id uint64) {
	c.id = id
	c.logger.SetPrefix(fmt.Sprintf("Client %d: ", id))
}
func (c *Client) SetCyph(cyph cipher.AEAD) {
	c.Cyph = cyph
}
func (c *Client) NewPlayer(player *objects.Player) {
	c.player = player
}
func (c *Client) SetUsername(username string) {
	c.username = username
}
func (c *Client) Username() string {
	fmt.Println(c.username)
	return c.username
}
func (c *Client) State() ClientStateHandler {
	return c.state
}
func (c *Client) Player() *objects.Player {
	return c.player
}
func (c *Client) Hub() *Hub {
	return c.hub
}
func (c *Client) SetState(newState ClientStateHandler, sync ...bool) {
	previousStateName := "None"
	if c.state != nil {
		previousStateName = c.state.Name()
		c.state.OnExit()
	}

	newStateName := "None"
	if newState != nil {
		newStateName = newState.Name()
	}

	c.logger.Printf("Changing state from %s to %s", previousStateName, newStateName)
	c.state = newState

	if c.state != nil {
		c.state.SetClient(c)
		c.state.OnEnter()
	}
}

func (c *Client) DbTx() *DbTx {
	return c.dbTx
}

func (c *Client) Id() uint64 {
	return c.id
}

func (c *Client) SocketSend(message packets.Msg, transferType TransferType) {
	c.SocketSendAs(message, c.id, transferType)
}
func (c *Client) SocketSendAs(message packets.Msg, senderId uint64, transferType TransferType) {
	switch transferType {
	case WebSocket:
		c.WS.SocketSendAs(message, senderId)
	default:
		c.logger.Printf("Unknown transfer type: %v", transferType)
	}
}

func (c *Client) Broadcast(message *packets.Packet, enet bool, ToAll bool) {
	c.hub.BroadcastChan <- &BroadcastItem{
		Packet: message,
		State:  c.state.Name(),
		ToAll:  ToAll,
		Enet:   enet,
	}
}
func (c *Client) RankedMMR() uint64 {
	return 0
}

func (c *Client) UnrankedMMR() uint64 {
	return 0
}
func (c *Client) SharedGameObjects() *SharedGameObjects {
	return c.hub.SharedGameObjects
}

func (c *Client) ProcessMessage(senderId uint64, message packets.Msg, transferType TransferType) {
	c.state.HandleMessage(senderId, message, transferType)
}

func (c *Client) GetAvatarImage() string {
	if c.IsSteamClient {
		image, err := c.dbTx.Queries.GetSteamAvatarImage(c.dbTx.Ctx, db.NewNullString(c.SteamID))
		if err != nil {
			c.logger.Printf("Failed to get steam avatar image: %v", err)
			return ""
		}
		return image.String
	}
	return ""
}
