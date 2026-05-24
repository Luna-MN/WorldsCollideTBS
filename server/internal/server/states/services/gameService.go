package services

import (
	"fmt"
	"math/rand"
	"server/internal/server"
	"sync"
)

type gameState int

const (
	Created gameState = iota
	StartGame
	InProgress
	Finished
)

type GameService struct {
	gameId       int64
	player1      *server.Client
	player2      *server.Client
	gameState    gameState
	ConfirmStart bool
	Mux          sync.RWMutex
}

func NewGameService(gameId int64) *GameService {
	return &GameService{gameId: gameId, gameState: Created}

}
func (g *GameService) Name() string {
	return "GameService"
}

func (g *GameService) Id() uint64 {
	return uint64(g.gameId)
}

func (g *GameService) SetClients(client1 *server.Client, client2 *server.Client) {
	g.player1 = client1
	g.player2 = client2
}
func (g *GameService) OnEnter() {
	g.Mux.Lock()
	defer g.Mux.Unlock()
	if !g.ConfirmStart {
		g.ConfirmStart = true
		return
	}
	g.gameState = StartGame
	fmt.Println("Game started")
}

func (g *GameService) GenerateSeed() int64 {
	return rand.Int63()
}
