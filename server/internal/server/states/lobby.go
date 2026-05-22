package states

import (
	"context"
	"fmt"
	"log"
	"server/internal/server"
	"server/internal/server/db"
	"server/internal/server/states/services"
	"server/pkg/packets"
	"strconv"
)

type Lobby struct {
	client      *server.Client
	logger      *log.Logger
	queries     *db.Queries
	dbCtx       context.Context
	auth        *services.AuthService
	hub         *server.Hub
	OtherClient *server.Client
	Queue       bool
}

func (l *Lobby) Name() string {
	return "Lobby"
}

func (l *Lobby) SetClient(client *server.Client) {
	if l.client != nil {
		l.OtherClient = client
	}
	l.client = client
	loginPrefix := fmt.Sprintf("Client %d [%s]: ", client.Id(), l.Name())
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
	if l.OtherClient != nil {
		if l.OtherClient.State().Name() == "Lobby" {
			l.OtherClient.SetState(&InGame{
				hub:         l.hub,
				OtherClient: l.client,
			})
			l.client.SetState(&InGame{
				hub:         l.hub,
				OtherClient: l.OtherClient,
			})
			otherId := strconv.FormatUint(l.OtherClient.Id(), 10)
			if l.OtherClient.SteamID != "" {
				otherId = l.OtherClient.SteamID
			}
			clientId := strconv.FormatUint(l.client.Id(), 10)
			if l.client.SteamID != "" {
				clientId = l.client.SteamID
			}
			l.client.SocketSendAs(packets.NewOpponent(otherId, l.OtherClient.IsSteamClient), l.OtherClient.Id(), server.WebSocket)
			l.OtherClient.SocketSendAs(packets.NewOpponent(clientId, l.client.IsSteamClient), l.client.Id(), server.WebSocket)
			if l.Queue {
				l.client.SocketSend(packets.NewStartGame(), server.WebSocket)
				l.OtherClient.SocketSend(packets.NewStartGame(), server.WebSocket)
			}
		}
	}
}

func (l *Lobby) HandleMessage(senderId uint64, message packets.Msg, transfer server.TransferType) {
	//TODO implement me
	panic("implement me")
}

func (l *Lobby) OnExit() {
}
