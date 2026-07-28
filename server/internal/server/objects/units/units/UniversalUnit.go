package units

import (
	"server/internal/server"
	"server/internal/server/combatDSL"
	"server/internal/server/objects"
	"server/internal/server/objects/tiles"
	"server/internal/server/objects/units/Movement"
	"server/internal/server/objects/units/Skills"
	"server/internal/server/objects/units/units/util"
	"server/pkg/packets"
)

type UniversalUnit struct {
	data        *util.UnitData
	pos         *objects.Vector3
	skills      []Skills.ISkill
	movement    Movement.IMovement
	Tile        *tiles.TerrainInfo
	SkillBuffer map[int32]combatDSL.CombatAction
	Inflictions map[int32]InflictAction
}

func (u *UniversalUnit) NewTurn() {
	for skillID, inflict := range u.Inflictions {
		inflict.Action.Execute(
			combatDSL.CombatContext{
				Caster: u,
				Target: u,
			}, skillID)

		inflict.Turns--
		if inflict.Turns <= 0 {
			delete(u.Inflictions, skillID)
		}
	}
}

func (u *UniversalUnit) RemoveInflict(skillID int32) {
	u.Inflictions[skillID].Action.Invert(skillID)
	delete(u.Inflictions, skillID)
	delete(u.SkillBuffer, skillID)
}

func (u *UniversalUnit) Inflict(skillID int32, action combatDSL.CombatAction, turns int) {
	u.Inflictions[skillID] = InflictAction{
		Action: action,
		Turns:  turns,
	}
}

func (u *UniversalUnit) AddToSkillBuffer(skillID int32, action combatDSL.CombatAction) {
	u.SkillBuffer[skillID] = action
}

func (u *UniversalUnit) RemoveFromSkillBuffer(skillId int32) {
	u.SkillBuffer[skillId].Invert(skillId)
	delete(u.SkillBuffer, skillId)
}

func (u *UniversalUnit) Heal(Amount int64) {
	//TODO implement me
	panic("implement me")
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
