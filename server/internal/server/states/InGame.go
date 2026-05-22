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

type InGame struct {
	client      *server.Client
	logger      *log.Logger
	queries     *db.Queries
	dbCtx       context.Context
	auth        *services.AuthService
	hub         *server.Hub
	OtherClient *server.Client
}

func (i InGame) Name() string {
	return "InGame"
}

func (i InGame) SetClient(client *server.Client) {
	if i.client != nil {
		i.OtherClient = client
	}
	i.client = client
	loginPrefix := fmt.Sprintf("Client %d [%s]: ", client.Id(), i.Name())
	i.logger = log.New(log.Writer(), loginPrefix, log.LstdFlags)
	i.queries = client.DbTx().Queries
	i.dbCtx = client.DbTx().Ctx
}

func (i InGame) CanReceiveGlobalChat() bool {
	return true
}

func (i InGame) OnEnter() {

}

func (i InGame) HandleMessage(senderId uint64, message packets.Msg, transfer server.TransferType) {
	//TODO implement me
	panic("implement me")
}

func (i InGame) OnExit() {
}
