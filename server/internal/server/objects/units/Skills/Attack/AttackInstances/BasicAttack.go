package AttackInstances

import (
	"server/internal/server"
	"server/internal/server/objects"
	"server/internal/server/objects/units/Skills"
)

type BasicAttack struct {
	unitId             int32
	clientId           uint64
	client             *server.Client
	enemyClient        *server.Client
	GameService        Skills.GameSkillService
	GameTerrainService Skills.GameTerrainService
}

func (b *BasicAttack) Type() Skills.SkillType {
	return Skills.Attack
}

func (b *BasicAttack) UnitId() int32 {
	return b.unitId
}

func (b *BasicAttack) ClientId() uint64 {
	return b.clientId
}

func (b *BasicAttack) Initiate(UnitId int32, client *server.Client, enemyClient *server.Client, gameService Skills.GameSkillService) {
	b.unitId = UnitId
	b.client = client
	b.enemyClient = enemyClient
	b.GameService = gameService
	b.GameTerrainService = gameService.World()
}

func (b *BasicAttack) ValidTarget(pos objects.Vector3) bool {
	//TODO implement me
	panic("implement me")
}
func (b *BasicAttack) Attack(pos objects.Vector3) {
	//TODO implement me
	panic("implement me")
}

func (b *BasicAttack) AttackValidation(pos objects.Vector3) {
	//TODO implement me
	panic("implement me")
}

func (b *BasicAttack) SendAttackPacket(pos objects.Vector3) {
	//TODO implement me
	panic("implement me")
}
