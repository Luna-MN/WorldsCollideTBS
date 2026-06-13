package units

import (
	"server/internal/server/objects"
	"server/internal/server/objects/units/Attack"
	"server/internal/server/objects/units/Movement"
	"server/internal/server/objects/units/units/util"
)

type IUnit interface {
	Position() objects.Vector3
	Data() *util.UnitData

	InitUnit()

	Attacks() []*Attack.IAttack
	Attack(IUnit)

	Movement() *Movement.IMovement
	Move(path []*objects.TerrainInfo)
}
