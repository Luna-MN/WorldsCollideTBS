package services

import (
	"context"
	"server/internal"
	"server/internal/server"
	"server/internal/server/db"
	"server/pkg/packets"
)

type GameDataService struct {
	hub     *server.Hub
	client  *server.Client
	queries *db.Queries
	dbCtx   context.Context
}

func (g *GameDataService) Name() string {
	return "GameDataService"
}

func (g *GameDataService) Id() uint64 {
	return 0
}

func (g *GameDataService) Version() string {
	return internal.GameDataVersion
}

func NewGameDataService(client *server.Client, hub *server.Hub, queries *db.Queries, dbCtx context.Context) *GameDataService {
	return &GameDataService{
		client:  client,
		hub:     hub,
		queries: queries,
		dbCtx:   dbCtx,
	}
}

func (g *GameDataService) HandleGameVersionUpdate() {
	// Get all Data from DB
	factions, err := g.queries.GetAllFactions(g.dbCtx)
	factionData := make([]*packets.FactionData, len(factions))
	if err != nil {
		return
	}
	armies, err := g.queries.GetAllArmies(g.dbCtx)
	armiesData := make([]*packets.ArmyData, len(armies))
	if err != nil {
		return
	}
	units, err := g.queries.GetAllUnits(g.dbCtx)
	unitsData := make([]*packets.UnitData, len(units))
	if err != nil {
		return
	}

	// map that data to packets
	for i, f := range factions {
		armyIds, err := g.queries.GetArmyIdsForFaction(g.dbCtx, f.ID)
		if err != nil {
			return
		}
		fac := packets.NewFactionDataFromDB(f, armyIds)
		factionData[i] = fac
	}
	for i, a := range armies {
		unitIds, err := g.queries.GetUnitIdsForArmy(g.dbCtx, a.ID)
		if err != nil {
			return
		}
		arm := packets.NewArmyDataFromDB(a, unitIds)
		armiesData[i] = arm
	}
	for i, u := range units {
		FID, err := g.queries.GetUnitsFaction(g.dbCtx, u.ID)
		if err != nil {
			return
		}
		uni := packets.NewUnitDataFromDB(u, FID)
		unitsData[i] = uni
	}

	// send that packet
	GD := packets.NewData(internal.GameDataVersion, factionData, armiesData, unitsData)
	g.client.SocketSend(GD, server.WebSocket)
}
