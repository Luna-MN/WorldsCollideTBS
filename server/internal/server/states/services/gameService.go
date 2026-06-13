package services

import (
	"fmt"
	"log"
	"math/rand"
	"server/internal/server"
	_ "server/internal/server/objects/features"
	"server/pkg/packets"
	"sync"
)

type gameState int

const (
	Created gameState = iota
	StartGame
	InProgress
	Finished
)

type PlayerGameData struct {
	Player *server.Client
	Seeds  []int32
	Seed   int32
}

func NewPlayerGameData(player *server.Client) *PlayerGameData {
	return &PlayerGameData{Player: player, Seeds: make([]int32, 3)}
}

type GameService struct {
	gameId       int64
	player1      *server.Client
	player2      *server.Client
	gameState    gameState
	ConfirmStart bool
	logger       *log.Logger
	Mux          sync.RWMutex

	Player1GameData *PlayerGameData
	Player2GameData *PlayerGameData

	seed          int32
	seedsReceived bool

	gameTerrainService *GameTerrainService
}

func NewGameService(gameId int64) *GameService {
	logger := log.New(log.Writer(), "Game unknown: ", log.LstdFlags)
	logger.SetPrefix(fmt.Sprintf("Game %d: ", gameId))
	return &GameService{gameId: gameId, gameState: Created, logger: logger}

}
func (g *GameService) Name() string {
	return "GameService"
}

func (g *GameService) Id() uint64 {
	return uint64(g.gameId)
}

func (g *GameService) SetClients(client1 *server.Client, client2 *server.Client) {
	g.player1 = client1
	g.Player1GameData = NewPlayerGameData(client1)
	g.player2 = client2
	g.Player2GameData = NewPlayerGameData(client2)

	g.logger.SetPrefix(fmt.Sprintf("Game %d: %s, %s", g.Id(), client1.Username(), client2.Username()))
}
func (g *GameService) GetClientData(client *server.Client) *PlayerGameData {
	if client == g.player1 {
		return g.Player1GameData
	}
	return g.Player2GameData
}
func (g *GameService) OnEnter() {
	g.Mux.Lock()
	defer g.Mux.Unlock()
	if !g.ConfirmStart {
		g.ConfirmStart = true
		return
	}
	g.gameState = StartGame
	g.logger.Println("Game started")
	g.GenerateGameSeeds()
}
func (g *GameService) GenerateGameSeeds() {
	g.GenerateSeeds(g.Player1GameData.Seeds)
	g.GenerateSeeds(g.Player2GameData.Seeds)
	g.seed = g.GenerateSeed()
	g.player1.SocketSend(packets.NewSeed(g.Player1GameData.Seeds), server.WebSocket)
	g.player2.SocketSend(packets.NewSeed(g.Player2GameData.Seeds), server.WebSocket)
}
func (g *GameService) GenerateSeed() int32 {
	return rand.Int31()
}

func (g *GameService) GenerateSeeds(seeds []int32) {
	for i := range seeds {
		seeds[i] = g.GenerateSeed()
	}
}

func (g *GameService) HandleMessage(player *server.Client, msg packets.Msg) {
	pd := g.GetClientData(player)
	switch msg.(type) {
	case *packets.Packet_Seed:
		g.HandleSeedMessage(pd, msg.(*packets.Packet_Seed))
	}
}

func (g *GameService) HandleSeedMessage(playerData *PlayerGameData, msg *packets.Packet_Seed) {
	g.Mux.Lock()
	defer g.Mux.Unlock()
	if g.gameState != StartGame {
		return
	}
	playerData.Seed = msg.Seed.Seed[0]
	if !g.seedsReceived {
		g.seedsReceived = true
		return
	}
	g.gameState = InProgress
	g.logger.Println("Seeds Selected, Starting Game")
	g.gameTerrainService = NewGameTerrainService(uint64(g.Player1GameData.Seed), uint64(g.Player2GameData.Seed), uint64(g.seed))
	g.gameTerrainService.GenerateTerrain()
	g.player1.SocketSend(packets.NewSeed([]int32{g.Player2GameData.Seed, g.seed}), server.WebSocket)
	g.player2.SocketSend(packets.NewSeed([]int32{g.Player1GameData.Seed, g.seed}), server.WebSocket)
}
