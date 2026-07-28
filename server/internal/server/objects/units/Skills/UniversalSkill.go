package Skills

import (
	"server/internal/server"
	"server/internal/server/combatDSL"
	"server/internal/server/objects"
	"server/pkg/packets"
)

type UniversalSkill struct {
	UnitId             int32
	ClientId           uint64
	Client             *server.Client
	EnemyClient        *server.Client
	GameService        GameSkillService
	GameTerrainService GameTerrainService
	data               Data
	ActionCompiler     *combatDSL.SkillCompiler
	Action             combatDSL.CombatAction
}

func (u *UniversalSkill) Type() packets.SkillType {
	return packets.SkillType_None
}

func (u *UniversalSkill) Name() string {
	return "UniversalSkill"
}

func (u *UniversalSkill) Initiate(UnitId int32, client *server.Client, enemyClient *server.Client, gameService GameSkillService) {
	u.UnitId = UnitId
	u.Client = client
	u.EnemyClient = enemyClient
	u.GameService = gameService
	u.GameTerrainService = gameService.World()
	u.ActionCompiler = combatDSL.NewSkillCompiler(nil)
}

func (u *UniversalSkill) CombatString(str string) {
	Action, err := u.ActionCompiler.CompileText(str)
	if err != nil {
		panic(err)
	}
	u.Action = Action

}
func (u *UniversalSkill) ValidTarget(pos objects.Vector2I) bool {
	return true
}

func (u *UniversalSkill) Use(skillId int32, pos objects.Vector2I) {
	tile := u.GameTerrainService.GetTileAt(pos)
	unit := tile.Unit

	u.Action.Execute(combatDSL.CombatContext{
		Caster: unit,
		Target: unit,
	}, skillId)
}

func (u *UniversalSkill) SendPacket(packet packets.Msg) {
	//TODO implement me
	panic("implement me")
}
