package services

import (
	"context"
	"server/internal/server"
	"server/internal/server/db"
	"server/internal/server/objects/units/Skills"
	"server/internal/server/objects/units/units"
	"server/internal/server/objects/units/units/util"
	"server/pkg/packets"
)

type PlayerFactionService struct {
	gameService *GameService
	client      *server.Client

	ArmyId int64

	Units map[int32]units.IUnit

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

func (p *PlayerFactionService) InitUnitData(UnitIds *packets.Packet_UnitIds) {
	if UnitIds == nil || UnitIds.UnitIds == nil {
		p.gameService.logger.Println("InitUnitData received nil UnitIds packet")
		return
	}

	unitsDB, err := p.queries.GetUnitIdsForArmy(p.dbCtx, p.ArmyId)
	if err != nil {
		p.gameService.logger.Printf("Failed to get unit ids for army, %v", err)
		return
	}

	p.Units = make(map[int32]units.IUnit)
	PlayerIDs := make(map[int64][]int32)
	for _, message := range UnitIds.UnitIds.Ids {
		if message == nil {
			continue
		}
		PlayerIDs[message.Id] = append(PlayerIDs[message.Id], message.UnitId)
	}
	for _, unitID := range unitsDB {
		unitDB, err := p.queries.GetUnitById(p.dbCtx, unitID)
		if err != nil {
			p.gameService.logger.Printf("Failed to get unit by id, %v", err)
			continue
		}

		unitCount, err := p.queries.GetArmyUnitCount(p.dbCtx, db.GetArmyUnitCountParams{
			Armyid: p.ArmyId,
			Unitid: unitID,
		})
		if err != nil {
			p.gameService.logger.Printf("Failed to get army unit count, %v", err)
			continue
		}

		playerUnitIDs := PlayerIDs[unitID]
		if len(playerUnitIDs) < int(unitCount) {
			p.gameService.logger.Printf(
				"Not enough unit ids for unit %d: got %d, expected %d",
				unitID,
				len(playerUnitIDs),
				unitCount,
			)
			continue
		}

		for i := 0; i < int(unitCount); i++ {
			playerUnitID := playerUnitIDs[i]
			p.Units[playerUnitID] = units.NewUniversalUnit()
			oc := p.gameService.player1
			if p.client == p.gameService.player1 {
				oc = p.gameService.player2
			}
			var gs Skills.GameSkillService = p.gameService
			p.Units[playerUnitID].NewUnit(util.NewUnitDataFromDB(&unitDB, p.client.Id(), playerUnitID), p.client, oc, gs)
		}

	}
}
