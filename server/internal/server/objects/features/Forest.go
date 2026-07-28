package features

import (
	"server/internal/server/objects"
	"server/internal/server/objects/tiles"

	"github.com/furui/fastnoiselite-go"
)

type Forest struct {
	WorldInfo *tiles.WorldInfo
	args      *tiles.FeatureArgs
	noise     fastnoiselite.FastNoiseLite
	threshold float32
	tiles     []*tiles.TerrainInfo
}

func (f *Forest) Set(world *tiles.WorldInfo, args *tiles.FeatureArgs) {
	f.WorldInfo = world
	f.args = args
	f.threshold = args.Threshold
	f.noise = args.ForestNoise
	f.tiles = make([]*tiles.TerrainInfo, 0)
}

func (f *Forest) Generate(seed uint64) {
	f.noise.Seed = int32(seed)
	f.tiles = f.tiles[:0]

	for coord, tile := range f.WorldInfo.TerrainInfo {
		v := f.noise.GetNoise2D(
			fastnoiselite.FNLfloat(coord.X),
			fastnoiselite.FNLfloat(coord.Y),
		)

		if v > float64(f.threshold) {
			tile.TopTileType = objects.Tree
			tile.TileTopState = objects.Small

			f.WorldInfo.TerrainInfo[coord] = tile
			f.tiles = append(f.tiles, tile)
		}
	}

	for _, tile := range f.tiles {
		trees := 0

		for _, neighbor := range tile.Neighbors {
			if neighbor.TopTileType == objects.Tree {
				trees++
			}
		}

		if trees >= 4 {
			tile.TileTopState = objects.Large
		} else if trees >= 3 {
			tile.TileTopState = objects.Medium
		}
	}
}
func (f *Forest) Destroy() {
}

func (f *Forest) Update() {
}
