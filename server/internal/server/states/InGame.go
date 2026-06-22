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
	enemyClient *server.Client
	gameService *services.GameService
}

func (i *InGame) HandleStateMessage(s string, v ...any) {
}
func (i *InGame) Name() string {
	return "InGame"
}

func (i *InGame) SetClient(client *server.Client) {
	if i.client != nil {
		i.enemyClient = client
	} else {
		i.client = client
	}
	loginPrefix := fmt.Sprintf("%s, %s with %s ", i.client.Username(), i.Name(), i.enemyClient.Username())
	i.logger = log.New(log.Writer(), loginPrefix, log.LstdFlags)
	i.queries = i.client.DbTx().Queries
	i.dbCtx = i.client.DbTx().Ctx
}
func (i *InGame) CanReceiveGlobalChat() bool {
	return true
}

func (i *InGame) OnEnter() {
	i.gameService.OnEnter()
}

func (i *InGame) HandleMessage(senderId uint64, message packets.Msg, transfer server.TransferType) {
	i.gameService.HandleMessage(i.client, message)
}

func (i *InGame) OnExit() {
}
