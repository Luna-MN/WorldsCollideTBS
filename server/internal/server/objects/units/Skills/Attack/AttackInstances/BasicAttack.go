package AttackInstances

import (
	"server/internal/server"
	"server/internal/server/objects"
	"server/internal/server/objects/units/Skills"
	"server/pkg/packets"
)

type BasicAttack struct {
	unitId             int32
	clientId           uint64
	client             *server.Client
	enemyClient        *server.Client
	GameService        Skills.GameSkillService
	GameTerrainService Skills.GameTerrainService
	data               Skills.AttackData
}

func (b *BasicAttack) Type() packets.SkillType {
	return packets.SkillType_Attack
}
func (b *BasicAttack) Name() string {
	return "BasicAttack"
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

func (b *BasicAttack) Use(skillId int32, pos objects.Vector2I) {
	unitId := b.GameTerrainService.GetTileAt(pos).Unit
	if unitId == -1 {
		b.SendPacket(packets.NewSkillDenyMessage(skillId, b.ClientId(), "No unit at that position"))
		return
	}
	unit := b.GameService.GetUnit(b.ClientId(), unitId)
	if b.ValidTarget(pos) {
		unit.Damage(int64(b.data.Damage))
		b.SendPacket(packets.NewSkillAcceptMessage(skillId, b.ClientId()))

	} else {
		b.SendPacket(packets.NewSkillDenyMessage(skillId, b.ClientId(), "No unit at that position"))
	}
}

func (b *BasicAttack) ValidTarget(pos objects.Vector2I) bool {
	return true
}

func (b *BasicAttack) SendPacket(packet packets.Msg) {
	b.GameService.SendToClients(packet)
}
