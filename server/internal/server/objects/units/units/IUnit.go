package units

import (
	"server/internal/server"
	"server/internal/server/combatDSL"
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

	NewTurn()

	Movement() *Movement.IMovement
	Move(path []*packets.HexPositionMessage) bool

	Damage(Amount int64)
	Heal(Amount int64)

	AddToSkillBuffer(skillID int32, action combatDSL.CombatAction)
	RemoveFromSkillBuffer(skillId int32)
	Inflict(skillID int32, action combatDSL.CombatAction, turns int)
	RemoveInflict(skillID int32)
}

type InflictAction struct {
	Action combatDSL.CombatAction
	Turns  int
}
