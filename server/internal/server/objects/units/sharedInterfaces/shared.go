package sharedInterfaces

import (
	"server/internal/server/objects"
	"server/internal/server/objects/tiles"
)

type MoveTerrainService interface {
	GetTileAt(pos objects.Vector2I) *tiles.TerrainInfo
}
