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
	client       *server.Client
	logger       *log.Logger
	queries      *db.Queries
	dbCtx        context.Context
	auth         *services.AuthService
	hub          *server.Hub
	LobbyService *services.LobbyService
	OtherClient  *server.Client
	Queue        bool
}

func (l *Lobby) HandleStateMessage(s string, v ...any) {
	if s == "StartGame" {
		l.client.SetState(&InGame{
			hub:         l.hub,
			enemyClient: l.OtherClient,
			gameService: v[0].(*services.GameService),
		})
	}
}

func (l *Lobby) Name() string {
	return "Lobby"
}

func (l *Lobby) SetClient(client *server.Client) {
	if l.client != nil {
		l.UpdateOtherClient(client)
		return
	}
	l.client = client
	loginPrefix := fmt.Sprintf("%s : %d [%s]: ", client.Username(), client.Id(), l.Name())
	l.logger = log.New(log.Writer(), loginPrefix, log.LstdFlags)
	l.queries = client.DbTx().Queries
	l.dbCtx = client.DbTx().Ctx
}
func (l *Lobby) UpdateOtherClient(client *server.Client) {
	l.OtherClient = client
}
func (l *Lobby) UpdateLoggerPrefix(s string) {
	if s == "" {
		l.logger.SetPrefix(fmt.Sprintf("Client %d [%s]: ", l.client.Id(), l.Name()))
		return
	}
	l.logger.SetPrefix(fmt.Sprintf("Client %d [%s, %s]: ", l.client.Id(), l.Name(), s))
}
func (l *Lobby) CanReceiveGlobalChat() bool {
	return true
}

func (l *Lobby) OnEnter() {
	l.LobbyService.OnJoin(l.client)
}

func (l *Lobby) HandleMessage(senderId uint64, message packets.Msg, transfer server.TransferType) {

}

func (l *Lobby) OnExit() {
}
