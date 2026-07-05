package services

import (
	"context"
	"fmt"
	"log"
	"math/rand"
	"server/internal/server"
	"server/internal/server/db"
	"server/internal/server/objects"
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
	Player               *server.Client
	PlayerFactionService *PlayerFactionService
	ids                  *packets.Packet_UnitIds
	Seeds                []int32
	Seed                 int32
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

	ArmyIdsReceived       bool
	UnitPositionsReceived bool

	gameTerrainService *GameTerrainService
	seed               int32
	seedsReceived      bool

	turnManagementService *TurnManagementService

	queries *db.Queries
	dbCtx   context.Context
}

func NewGameService(gameId int64, queries *db.Queries, dbCtx context.Context) *GameService {
	logger := log.New(log.Writer(), "Game unknown: ", log.LstdFlags)
	logger.SetPrefix(fmt.Sprintf("Game %d: ", gameId))
	return &GameService{gameId: gameId, gameState: Created, logger: logger, queries: queries, dbCtx: dbCtx}

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
	g.Player1GameData.PlayerFactionService = NewPlayerFactionService(g, g.player1, g.queries, g.dbCtx)
	g.player2 = client2
	g.Player2GameData = NewPlayerGameData(client2)
	g.Player2GameData.PlayerFactionService = NewPlayerFactionService(g, g.player2, g.queries, g.dbCtx)

	g.logger.SetPrefix(fmt.Sprintf("Game %d: %s, %s", g.Id(), client1.Username(), client2.Username()))
}
func (g *GameService) GetClientData(client *server.Client) *PlayerGameData {
	if client == g.player1 {
		return g.Player1GameData
	}
	return g.Player2GameData
}

func (g *GameService) SendToClientsAs(senderID uint64, msg packets.Msg) {
	g.player1.SocketSendAs(msg, senderID, server.WebSocket)
	g.player2.SocketSendAs(msg, senderID, server.WebSocket)
}

func (g *GameService) SendToClients(msg packets.Msg) {
	g.player1.SocketSend(msg, server.WebSocket)
	g.player2.SocketSend(msg, server.WebSocket)
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
	case *packets.Packet_ArmyId:
		g.HandleArmyIdMessage(pd, msg.(*packets.Packet_ArmyId))
	case *packets.Packet_UnitIds:
		g.HandleUnitIdsMessage(pd, msg.(*packets.Packet_UnitIds))
	case *packets.Packet_UnitPositions:
		g.HandleUnitPositionsMessage(pd, msg.(*packets.Packet_UnitPositions))
	case *packets.Packet_HexPositions:
		g.HandleHexPositionsMessage(pd, msg.(*packets.Packet_HexPositions))
	}
}

func (g *GameService) HandleArmyIdMessage(pd *PlayerGameData, id *packets.Packet_ArmyId) {
	pd.PlayerFactionService.ArmyId = id.ArmyId.Id
}

func (g *GameService) SendArmyIDs(player *server.Client) {
	player.SocketSendAs(packets.NewArmyId(g.GetClientData(g.player1).PlayerFactionService.ArmyId), g.player1.Id(), server.WebSocket)
	player.SocketSendAs(packets.NewArmyId(g.GetClientData(g.player2).PlayerFactionService.ArmyId), g.player2.Id(), server.WebSocket)
}
func (g *GameService) HandleUnitIdsMessage(pd *PlayerGameData, ids *packets.Packet_UnitIds) {
	// assign the players units with the IDs

	if pd == nil {
		g.logger.Println("HandleUnitIdsMessage received nil PlayerGameData")
		return
	}
	if ids == nil || ids.UnitIds == nil {
		g.logger.Println("HandleUnitIdsMessage received nil UnitIds packet")
		return
	}
	if pd.PlayerFactionService == nil {
		g.logger.Println("HandleUnitIdsMessage received PlayerGameData with nil PlayerFactionService")
		return
	}

	g.Mux.Lock()
	defer g.Mux.Unlock()
	pd.PlayerFactionService.InitUnitData(ids)
	pd.ids = ids
	if !g.ArmyIdsReceived {
		g.ArmyIdsReceived = true
		return
	}
	g.logger.Println("All army IDs received")
	g.SendArmyIDs(g.player1)
	g.SendArmyIDs(g.player2)

	g.player1.SocketSend(g.Player2GameData.ids, server.WebSocket)
	g.player2.SocketSend(g.Player1GameData.ids, server.WebSocket)

	g.player1.SocketSend(packets.NewSeed(g.Player1GameData.Seeds), server.WebSocket)
	g.player2.SocketSend(packets.NewSeed(g.Player2GameData.Seeds), server.WebSocket)
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
	g.logger.Println("Seeds Selected, Starting Game")
	g.gameTerrainService = NewGameTerrainService(uint64(g.Player1GameData.Seed), uint64(g.Player2GameData.Seed), uint64(g.seed))
	g.gameTerrainService.GenerateTerrain()
	g.player1.SocketSend(packets.NewSeed([]int32{g.Player2GameData.Seed, g.seed}), server.WebSocket)
	g.player2.SocketSend(packets.NewSeed([]int32{g.Player1GameData.Seed, g.seed}), server.WebSocket)

}

func (g *GameService) HandleUnitPositionsMessage(pd *PlayerGameData, positions *packets.Packet_UnitPositions) {
	g.Mux.Lock()
	defer g.Mux.Unlock()
	genPos := objects.NewVector3(0, 0, 0)
	if pd.Player == g.player1 {
		genPos = objects.NewVector3(-13.8, 0, 0)
	} else {
		genPos = objects.NewVector3(13.8, 0, 0)
	}
	for _, unitPosition := range positions.UnitPositions.Units {
		// convert to global position
		pos := objects.HexToWorldPosition(int(unitPosition.Position.X), int(unitPosition.Position.Y))
		pos = pos.Add(genPos)
		pd.PlayerFactionService.Units[unitPosition.UnitId].SetPosition(&pos)
		//g.gameTerrainService.GetTileAt(packets.UnwrapVector2I(unitPosition.Position)).Unit = unitPosition.UnitId
	}
	g.logger.Println("Unit positions received for ", pd.Player.Username())
	if !g.UnitPositionsReceived {
		g.UnitPositionsReceived = true
		return
	}
	g.logger.Println("All unit positions received")
	g.SendUnitPositions(g.Player1GameData)
	g.SendUnitPositions(g.Player2GameData)
	g.gameState = InProgress
	g.InitTurnManagementService()
}

func (g *GameService) InitTurnManagementService() {
	g.turnManagementService = NewTurnManagementService(g.Player1GameData.Player.Id(), g.Player2GameData.Player.Id())
}

func (g *GameService) HandleHexPositionsMessage(pd *PlayerGameData, positions *packets.Packet_HexPositions) {

	unit, ok := pd.PlayerFactionService.Units[positions.HexPositions.Id]
	if !ok || unit == nil {
		g.logger.Printf("HandleHexPositionsMessage received unknown unit id %d", positions.HexPositions.Id)
		return
	}

	firstPosition := positions.HexPositions.Positions[0]
	lastPosition := positions.HexPositions.Positions[len(positions.HexPositions.Positions)-1]

	if firstPosition == nil || firstPosition.Position == nil {
		g.logger.Println("HandleHexPositionsMessage received nil first position")
		return
	}
	if lastPosition == nil || lastPosition.Position == nil {
		g.logger.Println("HandleHexPositionsMessage received nil last position")
		return
	}

	if unit.Move(positions.HexPositions.Positions) {
		startTile := g.gameTerrainService.GetTileAt(packets.UnwrapVector2I(firstPosition.Position))
		endTile := g.gameTerrainService.GetTileAt(packets.UnwrapVector2I(lastPosition.Position))

		if startTile == nil {
			g.logger.Println("HandleHexPositionsMessage start tile was nil")
			return
		}
		if endTile == nil {
			g.logger.Println("HandleHexPositionsMessage end tile was nil")
			return
		}

		startTile.Unit = 0
		endTile.Unit = unit.Data().UnitID
	}
}
func (g *GameService) SendUnitPositions(pd *PlayerGameData) {
	g.logger.Println(len(pd.PlayerFactionService.Units), " Units")
	unitPositions := make([]*packets.UnitPositionMessage, 0, len(pd.PlayerFactionService.Units))
	for _, unit := range pd.PlayerFactionService.Units {
		tile := g.gameTerrainService.GetGlobalTileAt(unit.Position())
		if tile == nil {
			g.logger.Printf("Warning: No tile found at position %v for unit %d\n", unit.Position(), unit.Data().UnitID)
			for _, tiles := range g.gameTerrainService.Tiles {
				g.logger.Printf("t=%+v", tiles.Position)
			}
			continue
		}
		posI := tile.PositionI
		unitPositions = append(unitPositions, packets.NewUnitPositionMessage(unit.Data().UnitID, posI))
	}
	g.logger.Println("Sending unit positions as ", pd.Player.Username(), " : ", unitPositions)
	g.SendToClientsAs(pd.Player.Id(), packets.NewUnitPositions(unitPositions))
}
