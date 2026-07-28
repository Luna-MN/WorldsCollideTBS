package tiles

import (
	"server/internal/server/objects"

	"github.com/furui/fastnoiselite-go"
)

type TerrainGen struct {
	Radius      int
	Amplitude   int
	Features    int
	GlobalPos   objects.Vector3
	noise       fastnoiselite.FastNoiseLite
	DefaultTile objects.TileType
	FeatureArgs *FeatureArgs
	world       *WorldInfo
	seed        uint64
}

func NewTerrainGen(radius, amplitude, features int, DefaultTile objects.TileType, noise fastnoiselite.FastNoiseLite) *TerrainGen {
	return &TerrainGen{
		Radius:      radius,
		Amplitude:   amplitude,
		Features:    features,
		DefaultTile: DefaultTile,
		world:       NewWorldInfo(radius, amplitude, features, DefaultTile),
		noise:       noise,
	}
}

func (t *TerrainGen) GenerateTerrainInfo(seed uint64) {
	t.world.GenerateTerrainInfo(t.noise, seed)
}

func (t *TerrainGen) GetWorldInfo() *WorldInfo {
	return t.world
}

func (t *TerrainGen) GetTileAt(pos objects.Vector2I) *TerrainInfo {
	return t.world.TerrainInfo[pos]
}
