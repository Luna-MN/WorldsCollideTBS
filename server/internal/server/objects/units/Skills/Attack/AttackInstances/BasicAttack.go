package AttackInstances

import (
	"server/internal/server"
	"server/internal/server/objects"
	"server/internal/server/objects/units/Skills"
	"server/pkg/packets"
)

type BasicAttack struct {
	Skills.UniversalSkill
}

func (b *BasicAttack) Type() packets.SkillType {
	return packets.SkillType_Attack
}
func (b *BasicAttack) Name() string {
	return "BasicAttack"
}

func (b *BasicAttack) Initiate(UnitId int32, client *server.Client, enemyClient *server.Client, gameService Skills.GameSkillService) {
	b.UniversalSkill.Initiate(UnitId, client, enemyClient, gameService)
	b.CombatString("D(3)")
}

func (b *BasicAttack) Use(skillId int32, pos objects.Vector2I) {
	if b.ValidTarget(pos) {
		b.UniversalSkill.Use(skillId, pos)
		b.SendPacket(packets.NewSkillAcceptMessage(skillId, b.ClientId))

	} else {
		b.SendPacket(packets.NewSkillDenyMessage(skillId, b.ClientId, "No unit at that position"))
	}
}

func (b *BasicAttack) ValidTarget(pos objects.Vector2I) bool {
	return true
}

func (b *BasicAttack) SendPacket(packet packets.Msg) {
	b.GameService.SendToClients(packet)
}
