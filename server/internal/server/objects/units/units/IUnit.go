package units

import (
	"server/internal/server"
	"server/internal/server/objects"
	"server/internal/server/objects/units/Movement"
	"server/internal/server/objects/units/Skills"
	"server/internal/server/objects/units/units/util"
	"server/pkg/packets"
)

type IUnit interface {
	NewUnit(*util.UnitData, *server.Client, *server.Client, Skills.GameSkillService)
	Position() objects.Vector3
	SetPosition(*objects.Vector3)
	Data() *util.UnitData

	Skills() []Skills.ISkill
	Attack(IUnit)

	Movement() *Movement.IMovement
	Move(path []*packets.HexPositionMessage) bool

	Damage(Amount int64)
}
