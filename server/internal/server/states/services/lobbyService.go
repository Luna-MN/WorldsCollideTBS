package services

import (
	"context"
	"server/internal/server"
	"server/internal/server/db"
	"server/pkg/packets"
	"strconv"
	"sync"
	"time"
)

type LobbyService struct {
	lobbyId    uint64
	hub        *server.Hub
	client1    *server.Client
	client2    *server.Client
	queueLobby bool
	queries    *db.Queries
	dbCtx      context.Context
	Init       bool
	Mux        sync.RWMutex
}

func NewLobbyService(queueLobby bool, hub *server.Hub, queries *db.Queries, dbCtx context.Context) *LobbyService {
	return &LobbyService{queueLobby: queueLobby, hub: hub, queries: queries, dbCtx: dbCtx, Init: true}
}
func (l *LobbyService) SetClients(client1 *server.Client, client2 *server.Client) {
	l.client1 = client1
	l.client2 = client2
}
func (l *LobbyService) GetClients() (client1 *server.Client, client2 *server.Client) {
	return l.client1, l.client2
}
func (l *LobbyService) SetLobbyId(lobbyId uint64) {
	l.lobbyId = lobbyId
}
func (l *LobbyService) Name() string {
	return "LobbyService"
}

func (l *LobbyService) Id() uint64 {
	return l.lobbyId
}

func (l *LobbyService) OnJoin(client *server.Client) {
	l.Mux.Lock()
	defer l.Mux.Unlock()
	if l.client1 == nil {
		l.client1 = client
		return
	}
	l.client2 = client

	if l.client1 == nil || l.client2 == nil {
		return
	}

	otherId := strconv.FormatUint(l.client2.Id(), 10)
	if l.client2.SteamID != "" {
		otherId = l.client2.SteamID
	}
	clientId := strconv.FormatUint(l.client1.Id(), 10)
	if l.client1.SteamID != "" {
		clientId = l.client1.SteamID
	}
	l.client1.SocketSend(packets.NewIds([]uint64{l.client1.Id(), l.client2.Id()}), server.WebSocket)
	l.client2.SocketSend(packets.NewIds([]uint64{l.client1.Id(), l.client2.Id()}), server.WebSocket)
	l.client1.SocketSendAs(packets.NewOpponent(otherId, l.client2.IsSteamClient, l.client2.GetAvatarImage()), l.client2.Id(), server.WebSocket)
	l.client2.SocketSendAs(packets.NewOpponent(clientId, l.client1.IsSteamClient, l.client1.GetAvatarImage()), l.client1.Id(), server.WebSocket)
	if l.queueLobby {
		l.StartGame()
	}
}

func (l *LobbyService) StartGame() {
	game, err := l.queries.NewGame(l.dbCtx, db.NewGameParams{
		Player1id:    db.NewNullInt64(int64(l.client1.Id())),
		Player2id:    db.NewNullInt64(int64(l.client2.Id())),
		Player1score: db.NewNullInt64(0),
		Player2score: db.NewNullInt64(0),
		Winnerid:     db.NewNullInt64(0),
		Matchtime:    db.NewNullTime(time.Now()),
	})
	if err != nil {
		return
	}
	gameService := NewGameService(game.ID, l.queries, l.dbCtx)
	gameService.SetClients(l.client1, l.client2)
	l.hub.Games.Add(gameService)
	l.SendToClients(packets.NewStartGame())
	l.hub.Lobbies.Remove(l.Id())
	l.client1.State().HandleStateMessage("StartGame", gameService)
	l.client2.State().HandleStateMessage("StartGame", gameService)
}

func (l *LobbyService) SendToClients(msg packets.Msg) {
	l.client1.SocketSendAs(msg, l.client1.Id(), server.WebSocket)
	l.client2.SocketSendAs(msg, l.client2.Id(), server.WebSocket)
}
