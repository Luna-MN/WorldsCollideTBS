package Skills

import (
	"server/internal/server/objects"
	"server/internal/server/objects/units/units"
	"server/pkg/packets"
)

type GameTerrainService interface {
	GetTileAt(pos objects.Vector2I) *objects.TerrainInfo
	GetTiles(positions []objects.Vector2I) []*objects.TerrainInfo
	GetGlobalTileAt(pos objects.Vector3) *objects.TerrainInfo
	GetClosestGlobalTile(pos objects.Vector3) *objects.TerrainInfo
	GetGlobalTilesAt(positions []objects.Vector3) []*objects.TerrainInfo
}

type GameSkillService interface {
	SendToClients(msg packets.Msg)
	SendToClientsAs(senderId uint64, msg packets.Msg)
	World() GameTerrainService
	GetUnit(clientId uint64, id int32) units.IUnit
}

type SkillFactory func() ISkill

var SkillRegistry = map[string]ISkill{}

func RegisterSkill(name string, factory SkillFactory) {
	SkillRegistry[name] = factory()
}

func init() {

}
