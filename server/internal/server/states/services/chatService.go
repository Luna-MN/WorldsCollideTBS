package services

import (
	"fmt"
	"server/internal/server"
	"server/pkg/packets"
)

type ChatService struct {
	client *server.Client
	hub    *server.Hub
}

func NewChatService(client *server.Client, hub *server.Hub) *ChatService {
	return &ChatService{client: client, hub: hub}
}

func (c *ChatService) HandleChatMessage(senderId uint64, msg *packets.Packet_Chat) {
	if senderId == c.client.Id() {
		c.SendMessage(senderId, msg)
		return
	}
	sender, _ := c.hub.Clients.Get(senderId)
	if sender == nil {
		fmt.Println("Sender not found")
		return
	}
	switch msg.Chat.Type {
	case packets.ChatType_Nothing:
		return
	case packets.ChatType_Global:
		c.HandleGlobalMessage(sender, msg)
	case packets.ChatType_Local:
		c.HandleLocalMessage(sender, msg)
	case packets.ChatType_Party:
	//Add More functionality here
	case packets.ChatType_Max:
		return

	}
}

func (c *ChatService) SendMessage(senderId uint64, msg *packets.Packet_Chat) {
	p := &packets.Packet{
		Msg:      msg,
		SenderId: senderId,
	}
	c.client.Broadcast(p, false, true)
}
func (c *ChatService) HandleGlobalMessage(sender *server.Client, msg *packets.Packet_Chat) {
	if c.client.State().CanReceiveGlobalChat() {
		c.client.SocketSendAs(msg, sender.Id(), server.WebSocket)
	}
}
func (c *ChatService) HandleLocalMessage(sender *server.Client, msg *packets.Packet_Chat) {
	if sender.State().Name() == c.client.State().Name() {
		c.client.SocketSendAs(msg, sender.Id(), server.WebSocket)
	}
}
func (c *ChatService) HandleCommandMessage(senderId int64, msg string) error {
	// this is for dev commands comming through chat
	return nil
}
