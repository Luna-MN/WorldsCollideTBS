package states

import (
	"context"
	"fmt"
	"log"
	"math/rand"
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
	if m.inQueue {
		m.logger.Printf("Already in queue, ignoring")
		return
	}
	var mmr uint64
	if QueueType == "ranked" {
		m.queue = m.hub.RankedQueue
		mmr = m.client.RankedMMR()
	} else {
		m.queue = m.hub.UnrankedQueue
		mmr = m.client.UnrankedMMR()
	}
	var queueWithinBounds = m.queue.WithinMMRBounds(mmr)
	if len(queueWithinBounds) > 0 {
		m.logger.Printf("Queue within bounds: %v", queueWithinBounds)
		// get random player from queue
		var p objects.QueueClient
		for _, queuedPlayer := range queueWithinBounds {
			p = queuedPlayer
			break
		}

		m.queue.Remove(p.Id)
		if len(queueWithinBounds) > 1 {
			p = queueWithinBounds[uint64(rand.Intn(len(queueWithinBounds)))]
		}

		if p.ClientId == m.client.Id() {
			m.logger.Printf("Client is Me")
		}
		oc, _ := m.hub.Clients.Get(p.ClientId)
		if oc == nil {
			m.logger.Printf("Client not found")
			return
		}
		if oc.State().Name() == "Lobby" {
			oc.State().SetClient(m.client)
		}
		oc.SocketSend(packets.NewQueue("found"), server.WebSocket)
		oc.SetState(&Lobby{
			hub:         m.hub,
			OtherClient: m.client,
			Queue:       true,
		})
		m.logger.Printf("Client %d is in queue, joining lobby", oc.Id())
		m.client.SocketSend(packets.NewQueue("found"), server.WebSocket)
		m.client.SetState(&Lobby{
			hub:         m.hub,
			OtherClient: oc,
			Queue:       true,
		})
		return
	}
	m.logger.Printf("Queue not within bounds, adding to queue: %v", QueueType)
	// if no queue within bounds, add to queue
	m.queue.Add(objects.NewQueueClient(m.queue.GetNextId(), m.client.Id(), mmr), m.client.Id())
	m.inQueue = true
	m.UpdateLoggerPrefix(fmt.Sprintf("Queueing for %s", QueueType))
	m.client.SocketSend(packets.NewQueue(QueueType), server.WebSocket)
}
func (m *Menu) OnExit() {
	if m.inQueue {
		m.client.SocketSend(packets.NewQueue("exit"), server.WebSocket)
		m.inQueue = false
		m.queue.Remove(m.client.Id())
	}
}
