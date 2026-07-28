package tiles

import (
	"fmt"
	"math"
	"server/internal/server/combatDSL"
	"server/internal/server/objects"
	noise2 "server/internal/server/objects/noise"
	"server/internal/server/objects/rng"
	"server/internal/server/objects/units/Movement"
	"server/internal/server/objects/units/units/util"
	"server/pkg/packets"

	"github.com/furui/fastnoiselite-go"
)

var HexNeighborOffsets = []objects.Vector2I{
	{1, 0},
	{1, -1},
	{0, -1},
	{-1, 0},
	{-1, 1},
	{0, 1},
}

type TerrainInfo struct {
	TileHeight   int
	TileType     objects.TileType
	TopTileType  objects.TopTileType
	TileTopState objects.TileTopState
	PositionI    objects.Vector2I
	PositionL    objects.Vector2I
	Position     objects.Vector3
	Neighbors    []*TerrainInfo
	MovementCost int
	Unit         TileUnit
}

type TileUnit interface {
	Position() objects.Vector3
	SetPosition(*objects.Vector3)
	Data() *util.UnitData

	NewTurn()

	Movement() *Movement.IMovement
	Move(path []*packets.HexPositionMessage) bool

	Damage(Amount int64)
	Heal(Amount int64)

	AddToSkillBuffer(skillID int32, action combatDSL.CombatAction)
	RemoveFromSkillBuffer(skillId int32)
	Inflict(skillID int32, action combatDSL.CombatAction, turns int)
	RemoveInflict(skillID int32)
}

func NewTerrainInfo(tileType objects.TileType) *TerrainInfo {
	return &TerrainInfo{
		TileType: tileType,
	}
}

func (t *TerrainInfo) CalculateMovementCost() {
	t.MovementCost = 1
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
	TerrainInfo   map[objects.Vector2I]*TerrainInfo
	EdgeTiles     []*TerrainInfo
	DefaultTile   objects.TileType
	FeatureArgs   *FeatureArgs
	Radius        int
	Amplitude     int
	Features      int
	FeaturesArray []string
	FeaturesList  []IFeature
	Random        *rng.SplitMix64
}

func NewWorldInfo(radius, amplitude, features int, DefaultTile objects.TileType) *WorldInfo {
	return &WorldInfo{
		TerrainInfo: make(map[objects.Vector2I]*TerrainInfo),
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
		for r := r1; r <= r2; r++ {
			tileInfo := NewTerrainInfo(w.DefaultTile)
			tileInfo.PositionI = objects.Vector2I{r, q}
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

func HexToWorldPosition(q, r int) objects.Vector3 {
	hexSize := 1.15

	x := hexSize * (3.0 / 2.0 * float64(q))
	z := hexSize * (math.Sqrt(3)/2*float64(q) + math.Sqrt(3)*float64(r))
	return objects.Vector3{float32(x), 0, float32(z)}
}

func WorldToHexPosition(worldPos objects.Vector3) objects.Vector2I {
	hexSize := float32(1.15)

	q := worldPos.X * 2.0 / 3.0 / hexSize
	r := (worldPos.Z / hexSize / float32(math.Sqrt(3.0))) - (q / 2.0)

	qRounded, rRounded := HexRound(float64(q), float64(r))
	return objects.Vector2I{qRounded, rRounded}
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
		neighborCoord := objects.Vector2I{q + offset.X, r + offset.Y}
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

func (w *WorldInfo) IsEdgeTile(coord objects.Vector2I) bool {
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
