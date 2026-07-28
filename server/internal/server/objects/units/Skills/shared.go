package Skills

import (
	"server/internal/server/combatDSL"
	"server/internal/server/objects"
	"server/internal/server/objects/tiles"
	"server/pkg/packets"
)

type GameTerrainService interface {
	GetTileAt(pos objects.Vector2I) *tiles.TerrainInfo
	GetTiles(positions []objects.Vector2I) []*tiles.TerrainInfo
	GetGlobalTileAt(pos objects.Vector3) *tiles.TerrainInfo
	GetClosestGlobalTile(pos objects.Vector3) *tiles.TerrainInfo
	GetGlobalTilesAt(positions []objects.Vector3) []*tiles.TerrainInfo
}

type GameSkillService interface {
	SendToClients(msg packets.Msg)
	SendToClientsAs(senderId uint64, msg packets.Msg)
	World() GameTerrainService
	GetUnit(clientId uint64, id int32) GameUnit
}

type GameUnit interface {
	Damage(amount int64)
	Heal(amount int64)

	AddToSkillBuffer(skillID int32, action combatDSL.CombatAction)
	Inflict(skillID int32, action combatDSL.CombatAction, turns int)
	RemoveInflict(skillID int32)
}

type SkillFactory func() ISkill

var SkillRegistry = map[string]ISkill{}

func RegisterSkill(name string, factory SkillFactory) {
	SkillRegistry[name] = factory()
}

func init() {

}
