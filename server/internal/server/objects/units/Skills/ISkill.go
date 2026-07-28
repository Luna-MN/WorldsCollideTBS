package Skills

import (
	"server/internal/server"
	"server/internal/server/objects"
	"server/pkg/packets"
)

type ISkill interface {
	Type() packets.SkillType
	Name() string
	Initiate(UnitId int32, client *server.Client, enemyClient *server.Client, gameService GameSkillService)
	ValidTarget(pos objects.Vector2I) bool
	Use(skillId int32, pos objects.Vector2I)
	SendPacket(packet packets.Msg)
}

type Data struct {
	Cooldown    int
	AP          int
	Name        string
	Description string
	Type        packets.SkillType
	Range       int
}

type AttackData struct {
	Data
	Damage int
}
