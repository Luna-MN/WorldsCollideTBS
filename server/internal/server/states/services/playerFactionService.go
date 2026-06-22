package services

import (
	"context"
	"server/internal/server"
	"server/internal/server/db"
	"server/internal/server/objects/units/units/util"
)

type PlayerFactionService struct {
	gameService *GameService
	client      *server.Client

	ArmyId int64

	units map[int64]*util.UnitData

	queries *db.Queries
	dbCtx   context.Context
}

func NewPlayerFactionService(gameService *GameService, client *server.Client, queries *db.Queries, dbCtx context.Context) *PlayerFactionService {
	return &PlayerFactionService{
		gameService: gameService,
		client:      client,

		queries: queries,
		dbCtx:   dbCtx,
	}
}

func (p *PlayerFactionService) GetArmyId() int64 {
	return p.ArmyId
}

func (p *PlayerFactionService) InitUnitData() {
	units, err := p.queries.GetUnitIdsForArmy(p.dbCtx, p.ArmyId)
	if err != nil {
		p.gameService.logger.Printf("Failed to get unit ids for army, %v", err)
		return
	}
	p.units = make(map[int64]*util.UnitData)
	for _, unitID := range units {
		unitDB, err := p.queries.GetUnitById(p.dbCtx, unitID)
		if err != nil {
			p.gameService.logger.Printf("Failed to get unit by id, %v", err)
			continue
		}
		p.units[unitID] = util.NewUnitDataFromDB(&unitDB, p.client.Id())
	}
}
