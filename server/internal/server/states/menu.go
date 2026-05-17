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

type Menu struct {
	client  *server.Client
	logger  *log.Logger
	queries *db.Queries
	dbCtx   context.Context
	auth    *services.AuthService
	hub     *server.Hub
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
}

func (m *Menu) CanReceiveGlobalChat() bool {
	return true
}

func (m *Menu) OnEnter() {}

func (m *Menu) HandleMessage(senderId uint64, message packets.Msg, transfer server.TransferType) {

}

func (m *Menu) HandleQueueMessage(message packets.Msg) {

}
func (m *Menu) OnExit() {}
