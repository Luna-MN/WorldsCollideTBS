package services

import (
	"context"
	"log"
	"math/rand"
	"server/internal/server"
	"server/internal/server/db"
	"server/internal/server/objects"
	"server/pkg/packets"
)

type QueueService struct {
	client  *server.Client
	hub     *server.Hub
	queue   *objects.QueueCollection[objects.QueueClient]
	inQueue bool
	logger  *log.Logger
	queries *db.Queries
	dbCtx   context.Context
}

func (q QueueService) Name() string {
	return "QueueService"
}

func (q QueueService) Id() uint64 {
	return 0
}

func NewQueueService(client *server.Client, hub *server.Hub, queries *db.Queries, logger *log.Logger, dbCtx context.Context) *QueueService {
	return &QueueService{
		client:  client,
		hub:     hub,
		inQueue: false,
		logger:  logger,
		queries: queries,
		dbCtx:   dbCtx,
	}
}

func (q QueueService) QueueHandler(QueueType string) (*LobbyService, *server.Client) {
	var mmr uint64
	if QueueType == "ranked" {
		q.queue = q.hub.RankedQueue
		mmr = q.client.RankedMMR()
	} else {
		q.queue = q.hub.UnrankedQueue
		mmr = q.client.UnrankedMMR()
	}
	var queueWithinBounds = q.queue.WithinMMRBounds(mmr)
	if len(queueWithinBounds) > 0 {
		q.logger.Printf("Queue within bounds: %v", queueWithinBounds)
		// get random player from queue
		var p objects.QueueClient
		for _, queuedPlayer := range queueWithinBounds {
			p = queuedPlayer
			break
		}

		q.queue.Remove(p.ClientId)
		if len(queueWithinBounds) > 1 {
			p = queueWithinBounds[uint64(rand.Intn(len(queueWithinBounds)))]
		}

		if p.ClientId == q.client.Id() {
			q.logger.Printf("Client is Me")
			return &LobbyService{}, &server.Client{}
		}
		oc, _ := q.hub.Clients.Get(p.ClientId)
		if oc == nil {
			q.logger.Printf("Client not found")
			return &LobbyService{}, &server.Client{}
		}
		if oc.State().Name() == "Lobby" {
			oc.State().SetClient(q.client)
		}
		ls := NewLobbyService(true, q.hub, q.queries, q.dbCtx)
		lid := q.hub.Lobbies.Add(ls)
		ls.SetLobbyId(lid)
		q.logger.Printf("Client %d is in queue, joining lobby", oc.Id())
		oc.SocketSend(packets.NewQueue("found"), server.WebSocket)
		q.client.SocketSend(packets.NewQueue("found"), server.WebSocket)
		return ls, oc
	}
	q.logger.Printf("Queue not within bounds, adding to queue: %v", QueueType)
	// if no queue within bounds, add to queue
	q.queue.Add(objects.NewQueueClient(q.queue.GetNextId(), q.client.Id(), mmr), q.client.Id())
	q.inQueue = true
	q.client.SocketSend(packets.NewQueue(QueueType), server.WebSocket)
	return &LobbyService{}, &server.Client{}
}
