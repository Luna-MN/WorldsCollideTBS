package states

import (
	"context"
	"fmt"
	"log"
	"server/internal"
	"server/internal/server"
	"server/internal/server/db"
	"server/internal/server/states/services"
	"server/pkg/packets"
	"time"
)

type Login struct {
	client   *server.Client
	logger   *log.Logger
	queries  *db.Queries
	dbCtx    context.Context
	gameData *services.GameDataService
	auth     *services.AuthService
	hub      *server.Hub
}

func (l *Login) HandleStateMessage(s string, v ...any) {
}

func (l *Login) CanReceiveGlobalChat() bool {
	return false
}

func (l *Login) Name() string {
	return "Login"
}

func (l *Login) SetClient(client *server.Client) {
	l.client = client
	loginPrefix := fmt.Sprintf("%s [%d]: [%s] ", client.Username(), client.Id(), l.Name())
	l.logger = log.New(log.Writer(), loginPrefix, log.LstdFlags)
	l.queries = client.DbTx().Queries
	l.dbCtx = client.DbTx().Ctx
	l.gameData = services.NewGameDataService(client, l.client.Hub(), l.queries, l.dbCtx)
}

func (l *Login) OnEnter() {
	l.auth = services.NewAuthService(l.client.DbTx())
	l.client.SocketSend(packets.NewId(l.client.Id()), server.WebSocket)
}

func (l *Login) HandleMessage(senderId uint64, message packets.Msg, transfer server.TransferType) {
	// don't accept traffic from enet
	if transfer != server.WebSocket {
		return
	}
	switch message := message.(type) {
	case *packets.Packet_GameVersion:
		l.HandleGameDataVersion(senderId, message)
	case *packets.Packet_LoginRequest:
		l.HandleLoginRequest(senderId, message)
	case *packets.Packet_RegisterRequest:
		l.HandleRegisterRequest(senderId, message)
	case *packets.Packet_SteamTicket:
		l.HandleSteamTicket(senderId, message)
	}
}

func (l *Login) OnExit() {
}

func (l *Login) HandleGameDataVersion(id uint64, message *packets.Packet_GameVersion) {
	if message.GameVersion.Version == internal.GameDataVersion {
		return
	}
	l.gameData.HandleGameVersionUpdate()
}

func (l *Login) HandleLoginRequest(id uint64, message *packets.Packet_LoginRequest) {
	if id != l.client.Id() {
		l.logger.Printf("Received login request from different client, ignoring (ID %d)", id)
		return
	}

	username := message.LoginRequest.Username

	msg, err := l.auth.Login(username, message.LoginRequest.Password)
	l.client.SocketSend(msg, server.WebSocket)
	if err != nil {
		l.logger.Printf("Login failed for user %s: %v", username, err)
		return
	}

	l.logger.Printf("Login successful for user %s", username)
	l.client.SetUsername(username)
	// change state to in game
	l.client.SetState(&Menu{})
}

func (l *Login) HandleRegisterRequest(id uint64, message *packets.Packet_RegisterRequest) {
	if id != l.client.Id() {
		l.logger.Printf("Received register request from different client, ignoring (ID %d)", id)
		return
	}

	username := message.RegisterRequest.Username

	msg, err := l.auth.Register(username, message.RegisterRequest.Password)
	l.client.SocketSend(msg, server.WebSocket)
	if err != nil {
		l.logger.Printf("Error registering user: %v", err)
		return
	}

	l.logger.Printf("User Registered %s", username)
}

func (l *Login) HandleSteamTicket(id uint64, message *packets.Packet_SteamTicket) {
	p, user, err := l.auth.SteamLogin(string(message.SteamTicket.Ticket), l.client.Steam)
	if err != nil {
		l.logger.Printf("Error logging in with steam: %v", err)
	}
	l.client.SteamID = user.Steamid.String
	l.client.IsSteamClient = true
	summ, err := l.client.Steam.GetPlayerSummaries(l.dbCtx, user.Steamid.String)
	if err != nil {
		l.logger.Printf("Error getting steam user info: %v", err)
	}
	if summ.Response.Players[0].PersonaName != user.Username {
		l.client.SetUsername(summ.Response.Players[0].PersonaName)
	}
	if db.NewNullString(summ.Response.Players[0].Avatar) != user.Avatar {
		_, err = l.queries.UpdateAvatar(l.dbCtx, db.UpdateAvatarParams{
			Avatar: db.NewNullString(summ.Response.Players[0].Avatar),
			ID:     user.ID,
		})
		if err != nil {
			return
		}
	}
	_, err = l.queries.UpdateLastLoggedIn(l.dbCtx, db.UpdateLastLoggedInParams{
		ID:           user.ID,
		Lastloggedin: db.NewNullTime(time.Now()),
	})
	if err != nil {
		return
	}
	l.client.SetUsername(user.Username)
	l.client.SocketSend(p, server.WebSocket)
	if l.client.Username() != "" {
		l.client.SetState(&Menu{})
	}
}
