package objects

import (
	"github.com/furui/fastnoiselite-go"
)

type TerrainGen struct {
	Radius      int
	Amplitude   int
	Features    int
	GlobalPos   Vector3
	noise       fastnoiselite.FastNoiseLite
	DefaultTile TileType
	FeatureArgs *FeatureArgs
	world       *WorldInfo
	seed        uint64
}

func NewTerrainGen(radius, amplitude, features int, DefaultTile TileType, noise fastnoiselite.FastNoiseLite) *TerrainGen {
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

func (t *TerrainGen) GetTileAt(pos Vector2I) *TerrainInfo {
	return t.world.TerrainInfo[pos]
}
