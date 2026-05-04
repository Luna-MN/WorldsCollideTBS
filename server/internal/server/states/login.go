package states

import (
	"context"
	"fmt"
	"log"
	"server/internal/server"
	"server/internal/server/db"
	"server/internal/server/objects"
	"server/internal/server/states/services"
	"server/pkg/packets"
)

type Login struct {
	client  *server.Client
	logger  *log.Logger
	queries *db.Queries
	dbCtx   context.Context
	auth    *services.AuthService
	hub     *server.Hub
}

func (l *Login) CanReceiveGlobalChat() bool {
	return false
}

func (l *Login) Name() string {
	return "Login"
}

func (l *Login) SetClient(client *server.Client) {
	l.client = client
	loginPrefix := fmt.Sprintf("Client %d [%s]: ", client.Id(), l.Name())
	l.logger = log.New(log.Writer(), loginPrefix, log.LstdFlags)
	l.queries = client.DbTx().Queries
	l.dbCtx = client.DbTx().Ctx
}

func (l *Login) OnEnter() {
	l.auth = services.NewAuthService(l.client.DbTx())
}

func (l *Login) HandleMessage(senderId uint64, message packets.Msg, transfer server.TransferType) {
	// don't accept traffic from enet
	if transfer != server.WebSocket {
		return
	}
	switch message := message.(type) {
	case *packets.Packet_LoginRequest:
		l.HandleLoginRequest(senderId, message)
	case *packets.Packet_RegisterRequest:
		l.HandleRegisterRequest(senderId, message)
	}
}

func (l *Login) OnExit() {
}

func (l *Login) HandleLoginRequest(id uint64, message *packets.Packet_LoginRequest) {
	if id != l.client.Id() {
		l.logger.Printf("Received login request from differant client, ignoring (ID %d)", id)
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
	l.client.NewPlayer(&objects.Player{
		Name:  username,
		Owner: id,
	})
	// change state to in game
}

func (l *Login) HandleRegisterRequest(id uint64, message *packets.Packet_RegisterRequest) {
	if id != l.client.Id() {
		l.logger.Printf("Received register request from differant client, ignoring (ID %d)", id)
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
