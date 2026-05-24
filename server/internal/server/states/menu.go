package states

import (
	"context"
	"fmt"
	"log"
	"server/internal/server"
	"server/internal/server/db"
	"server/internal/server/objects"
	"server/internal/server/states/services"
	"server/pkg/packets"
)

type Menu struct {
	client  *server.Client
	logger  *log.Logger
	queries *db.Queries
	dbCtx   context.Context
	auth    *services.AuthService
	hub     *server.Hub
	inQueue bool
	queue   *objects.QueueCollection[objects.QueueClient]
}

func (m *Menu) HandleStateMessage(s string, v ...any) {
}

func (m *Menu) Name() string {
	return "Menu"
}

func (m *Menu) SetClient(client *server.Client) {
	m.client = client
	loginPrefix := fmt.Sprintf("Client %d [%s]: ", client.Id(), m.Name())
	m.logger = log.New(log.Writer(), loginPrefix, log.LstdFlags)
	m.queries = client.DbTx().Queries
	m.dbCtx = client.DbTx().Ctx
	m.hub = client.Hub()
}
func (m *Menu) UpdateLoggerPrefix(s string) {
	if s == "" {
		m.logger.SetPrefix(fmt.Sprintf("Client %d [%s]: ", m.client.Id(), m.Name()))
		return
	}
	m.logger.SetPrefix(fmt.Sprintf("Client %d [%s, %s]: ", m.client.Id(), m.Name(), s))
}
func (m *Menu) CanReceiveGlobalChat() bool {
	return true
}

func (m *Menu) OnEnter() {}

func (m *Menu) HandleMessage(senderId uint64, message packets.Msg, transfer server.TransferType) {
	switch message := message.(type) {
	case *packets.Packet_Queue:
		m.HandleQueueMessage(senderId, message.Queue)
	}
}

func (m *Menu) HandleQueueMessage(senderId uint64, message *packets.QueueMessage) {
	m.QueueHandler(message.QueueType)
}
func (m *Menu) QueueHandler(QueueType string) {
	qs := services.NewQueueService(m.client, m.hub, m.queries, m.logger, m.dbCtx)
	ls, oc := qs.QueueHandler(QueueType)
	if !ls.Init {
		m.UpdateLoggerPrefix(QueueType)
		return
	}
	oc.SetState(&Lobby{
		hub:          m.hub,
		OtherClient:  m.client,
		Queue:        true,
		LobbyService: ls,
	})
	m.client.SetState(&Lobby{
		hub:          m.hub,
		OtherClient:  m.client,
		Queue:        true,
		LobbyService: ls,
	})

}
func (m *Menu) OnExit() {
	if m.inQueue {
		m.client.SocketSend(packets.NewQueue("exit"), server.WebSocket)
		m.inQueue = false
		m.queue.Remove(m.client.Id())
	}
}
