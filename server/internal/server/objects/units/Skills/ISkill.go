package Skills

import (
	"server/internal/server"
	"server/internal/server/objects"
)

type SkillType int

const (
	None SkillType = iota
	Attack
	Support
)

type ISkill interface {
	Type() SkillType
	UnitId() int32
	ClientId() uint64
	ValidTarget(pos objects.Vector3) bool
	Initiate(UnitId int32, client *server.Client, enemyClient *server.Client, gameService GameSkillService)
}
