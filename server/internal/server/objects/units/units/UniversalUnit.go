package units

import (
	"server/internal/server"
	"server/internal/server/objects"
	"server/internal/server/objects/units/Movement"
	"server/internal/server/objects/units/Skills"
	"server/internal/server/objects/units/units/util"
	"server/pkg/packets"
)

type UniversalUnit struct {
	data     *util.UnitData
	pos      *objects.Vector3
	skills   []Skills.ISkill
	movement Movement.IMovement
	Tile     *objects.TerrainInfo
}

func NewUniversalUnit() *UniversalUnit {
	return &UniversalUnit{}
}

func (u *UniversalUnit) NewUnit(data *util.UnitData, client *server.Client, enemyClient *server.Client, service Skills.GameSkillService) {
	u.data = data
	// init attacks
	// init movement
	u.movement = Movement.MovementRegistry[data.Movement]()
	u.movement.InitMovement(data.UnitID, client, enemyClient)

	for _, skillName := range u.data.Skills { // change attacks to skills
		skill := Skills.SkillRegistry[skillName]
		skill.Initiate(data.UnitID, client, enemyClient, service)
		u.skills = append(u.skills, skill)
	}
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

func (u *UniversalUnit) Skills() []Skills.ISkill {
	return u.skills
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

func (u *UniversalUnit) Damage(Amount int64) {
	// check if hp is not negative
	if u.data.HP <= 0 {
		// kill unit
		return
	}
	u.data.HP -= Amount
}
