package objects

import (
	"fmt"
	"math"
	noise2 "server/internal/server/objects/noise"
	"server/internal/server/objects/rng"

	"github.com/furui/fastnoiselite-go"
)

var HexNeighborOffsets = []Vector2I{
	{1, 0},
	{1, -1},
	{0, -1},
	{-1, 0},
	{-1, 1},
	{0, 1},
}

type TerrainInfo struct {
	TileHeight   int
	TileType     TileType
	TopTileType  TopTileType
	TileTopState TileTopState
	PositionI    Vector2I
	Position     Vector3
	Neighbors    []*TerrainInfo
}

func NewTerrainInfo(tileType TileType) *TerrainInfo {
	return &TerrainInfo{
		TileType: tileType,
	}
}

type FeatureArgs struct {
	Threshold   float32
	ForestNoise fastnoiselite.FastNoiseLite
}

type IFeature interface {
	Set(world *WorldInfo, args *FeatureArgs)
	Generate(seed uint64)
	Destroy()
	Update()
}

type WorldInfo struct {
	TerrainInfo   map[Vector2I]*TerrainInfo
	EdgeTiles     []*TerrainInfo
	DefaultTile   TileType
	FeatureArgs   *FeatureArgs
	Radius        int
	Amplitude     int
	Features      int
	FeaturesArray []string
	FeaturesList  []IFeature
	Random        *rng.SplitMix64
}

func NewWorldInfo(radius, amplitude, features int, DefaultTile TileType) *WorldInfo {
	return &WorldInfo{
		TerrainInfo: make(map[Vector2I]*TerrainInfo),
		DefaultTile: DefaultTile,
		Radius:      radius,
		Amplitude:   amplitude,
		Features:    features,
	}
}

func (w *WorldInfo) GenerateTerrainInfo(noise fastnoiselite.FastNoiseLite, seed uint64) {
	// Initialize the terrain args
	w.Random = rng.NewSplitMix64(seed)
	w.FeatureArgs = &FeatureArgs{
		Threshold:   0.5,
		ForestNoise: noise2.ForestNoise,
	}
	for q := -w.Radius; q <= w.Radius; q++ {
		r1 := int(math.Max(float64(-w.Radius), float64(-q-w.Radius)))
		r2 := int(math.Min(float64(w.Radius), float64(-q+w.Radius)))
		for r := -r1; r <= r2; r++ {
			tileInfo := NewTerrainInfo(w.DefaultTile)
			tileInfo.PositionI = Vector2I{r, q}
			tileInfo.Position = HexToWorldPosition(q, r)
			w.TerrainInfo[tileInfo.PositionI] = tileInfo

			v := noise.GetNoise2D(fastnoiselite.FNLfloat(tileInfo.Position.X), fastnoiselite.FNLfloat(tileInfo.Position.Z))
			scaledValue := v * float64(w.Amplitude)
			val := int(math.Round(scaledValue*2) / 2)
			tileInfo.TileHeight = val
		}
	}
	for coord, terrainInfo := range w.TerrainInfo {
		neighbors := w.GetHexNeighbors(coord.X, coord.Y)
		terrainInfo.Neighbors = neighbors
	}
	w.EdgeTiles = w.GetEdgeTiles()
	for i := 0; i < w.Features; i++ {
		feature, err := w.GetRandomFeature()
		if err != nil {
			panic(err)
		}
		feature.Set(w, w.FeatureArgs)
		feature.Generate(seed)
		w.FeaturesList = append(w.FeaturesList, feature)
	}
}

func HexToWorldPosition(q, r int) Vector3 {
	hexSize := 1.15

	x := hexSize * (3 / 2 * float64(q))
	z := hexSize * (math.Sqrt(3)/2*float64(q) + math.Sqrt(3)*float64(r))
	return Vector3{float32(x), 0, float32(z)}
}

func WorldToHexPosition(x, z float32) Vector2I {
	hexSize := 1.15

	worldX := float64(x)
	worldZ := float64(z)

	qf := (2.0 / 3.0 * worldX) / hexSize
	rf := (-1.0/3.0*worldX + math.Sqrt(3.0)/3.0*worldZ) / hexSize

	q, r := HexRound(qf, rf)

	return Vector2I{q, r}
}

func HexRound(qf, rf float64) (int, int) {
	sf := -qf - rf

	q := math.Round(qf)
	r := math.Round(rf)
	s := math.Round(sf)

	qDiff := math.Abs(q - qf)
	rDiff := math.Abs(r - rf)
	sDiff := math.Abs(s - sf)

	if qDiff > rDiff && qDiff > sDiff {
		q = -r - s
	} else if rDiff > sDiff {
		r = -q - s
	}

	return int(q), int(r)
}
func (w *WorldInfo) GetHexNeighbors(q, r int) []*TerrainInfo {
	neighbors := make([]*TerrainInfo, 0, 6)

	for _, offset := range HexNeighborOffsets {
		neighborCoord := Vector2I{q + offset.X, r + offset.Y}
		if w.IsValidHexCoord(neighborCoord.X, neighborCoord.Y) {
			if neighbor := w.TerrainInfo[neighborCoord]; neighbor != nil {
				neighbors = append(neighbors, neighbor)
			}
		}
	}

	return neighbors
}
func (w *WorldInfo) IsValidHexCoord(q, r int) bool {
	return absInt(q) <= w.Radius && absInt(r) <= w.Radius && absInt(q+r) <= w.Radius
}

func absInt(v int) int {
	if v < 0 {
		return -v
	}
	return v
}

func (w *WorldInfo) GetEdgeTiles() []*TerrainInfo {
	edgeTiles := make([]*TerrainInfo, 0)
	for coord, terrainInfo := range w.TerrainInfo {
		if w.IsEdgeTile(coord) {
			edgeTiles = append(edgeTiles, terrainInfo)
		}
	}
	return edgeTiles
}

func (w *WorldInfo) IsEdgeTile(coord Vector2I) bool {
	neighbors := w.GetHexNeighbors(coord.X, coord.Y)
	return len(neighbors) < 6
}

func (w *WorldInfo) GetRandomFeature() (IFeature, error) {
	usedFeatures := make(map[string]bool, len(w.FeaturesArray))
	for _, name := range w.FeaturesArray {
		usedFeatures[name] = true
	}

	available := make([]string, 0)

	for name := range featureRegistry {
		if !usedFeatures[name] {
			available = append(available, name)
		}
	}

	if len(available) == 0 {
		return nil, fmt.Errorf("no implementations of IFeature found")
	}

	index := int(w.Random.Uint64() % uint64(len(available)))
	randomName := available[index]

	w.FeaturesArray = append(w.FeaturesArray, randomName)

	return featureRegistry[randomName](), nil
}
