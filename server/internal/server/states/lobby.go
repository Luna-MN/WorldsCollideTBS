package states

import (
	"context"
	"fmt"
	"log"
	"server/internal/server"
	"server/internal/server/db"
	"server/internal/server/states/services"
	"server/pkg/packets"
)

type Lobby struct {
	client  *server.Client
	logger  *log.Logger
	queries *db.Queries
	dbCtx   context.Context
	auth    *services.AuthService
	hub     *server.Hub
}

func (l *Lobby) Name() string {
	return "Lobby"
}

func (l *Lobby) SetClient(client *server.Client) {
	l.client = client
	loginPrefix := fmt.Sprintf("Client %d [%s]: ", client.Id(), l.Name())
	l.logger = log.New(log.Writer(), loginPrefix, log.LstdFlags)
	l.queries = client.DbTx().Queries
	l.dbCtx = client.DbTx().Ctx
}
func (l *Lobby) CanReceiveGlobalChat() bool {
	return true
}

func (l *Lobby) OnEnter() {
}

func (l *Lobby) HandleMessage(senderId uint64, message packets.Msg, transfer server.TransferType) {
	//TODO implement me
	panic("implement me")
}

func (l *Lobby) OnExit() {
}
