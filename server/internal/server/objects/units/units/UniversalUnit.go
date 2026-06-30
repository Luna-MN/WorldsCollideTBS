package units

import (
	"server/internal/server"
	"server/internal/server/objects"
	"server/internal/server/objects/units/Attack"
	"server/internal/server/objects/units/Movement"
	"server/internal/server/objects/units/units/util"
	"server/pkg/packets"
)

type UniversalUnit struct {
	data     *util.UnitData
	pos      *objects.Vector3
	attacks  []Attack.IAttack
	movement Movement.IMovement
	Tile     *objects.TerrainInfo
}

func NewUniversalUnit() *UniversalUnit {
	return &UniversalUnit{}
}

func (u *UniversalUnit) NewUnit(data *util.UnitData, client *server.Client, enemyClient *server.Client) {
	u.data = data
	// init attacks
	// init movement
	u.movement = Movement.MovementRegistry[data.Movement]()
	u.movement.InitMovement(data.UnitID, client, enemyClient)
}
func (u *UniversalUnit) Position() objects.Vector3 {
	return *u.pos
}

func (u *UniversalUnit) SetPosition(pos *objects.Vector3) {
	u.pos = pos
}

func (u *UniversalUnit) Data() *util.UnitData {
	return u.data
}

func (u *UniversalUnit) Attacks() *[]Attack.IAttack {
	return &u.attacks
}

func (u *UniversalUnit) Attack(unit IUnit) {
	//TODO implement me
	panic("implement me")
}

func (u *UniversalUnit) Movement() *Movement.IMovement {
	return &u.movement
}

func (u *UniversalUnit) Move(path []*packets.HexPositionMessage) bool {
	return u.movement.Move(path)
}
