package services

import (
	"fmt"
	"server/internal/server/objects"
	"server/internal/server/objects/noise"
)

var PlayerRadius = 2
var PlayerAmp = 2
var PlayerFeatures = 0

var MainRadius = 5
var MainAmp = 2
var MainFeatures = 2

type GameTerrainService struct {
	seed1       uint64
	terrainGen1 *objects.TerrainGen
	seed2       uint64
	terrainGen2 *objects.TerrainGen
	mainSeed    uint64
	terrainGen  *objects.TerrainGen
	Tiles       map[objects.Vector2I]*objects.TerrainInfo
}

func NewGameTerrainService(seed1, seed2, mainSeed uint64) *GameTerrainService {
	return &GameTerrainService{
		seed1:    seed1,
		seed2:    seed2,
		mainSeed: mainSeed,
		Tiles:    make(map[objects.Vector2I]*objects.TerrainInfo),
	}
}

func (g *GameTerrainService) Name() string {
	return "GameTerrainService"
}

func (g *GameTerrainService) Id() uint64 {
	return 0
}

func (g *GameTerrainService) GenerateTerrain() {
	g.terrainGen = objects.NewTerrainGen(MainRadius, MainAmp, MainFeatures, objects.Grass, noise.DefaultNoise)
	g.terrainGen1 = objects.NewTerrainGen(PlayerRadius, PlayerAmp, PlayerFeatures, objects.Fall, noise.PlayerDefaultNoise)
	g.terrainGen2 = objects.NewTerrainGen(PlayerRadius, PlayerAmp, PlayerFeatures, objects.Fall, noise.PlayerDefaultNoise)

	g.terrainGen.GenerateTerrainInfo(g.mainSeed)
	g.terrainGen1.GenerateTerrainInfo(g.seed1)
	g.terrainGen2.GenerateTerrainInfo(g.seed2)

	g.terrainGen.GlobalPos = objects.Vector3{}
	g.terrainGen1.GlobalPos = objects.Vector3{X: -13.8}
	g.terrainGen2.GlobalPos = objects.Vector3{X: 13.8}

	g.PopulateTiles()
}

func (g *GameTerrainService) GetTileAt(pos objects.Vector2I) *objects.TerrainInfo {
	return g.Tiles[pos]
}

func (g *GameTerrainService) GetTiles(positions []objects.Vector2I) []*objects.TerrainInfo {
	tiles := make([]*objects.TerrainInfo, len(positions))
	for i, pos := range positions {
		tiles[i] = g.GetTileAt(pos)
	}
	return tiles
}
func (g *GameTerrainService) GetGlobalTileAt(pos objects.Vector3) *objects.TerrainInfo {
	pos2D := objects.WorldToHexPosition(pos)
	tile := g.GetTileAt(pos2D)
	if tile == nil {
		tile = g.GetClosestGlobalTile(pos)
		fmt.Println("No tile found at ", pos2D, " using closest ", tile.PositionI)
	}
	return tile
}
func (g *GameTerrainService) GetClosestGlobalTile(pos objects.Vector3) *objects.TerrainInfo {
	var closest *objects.TerrainInfo
	minDist := float32(1e9)

	for _, tile := range g.Tiles {
		dx := tile.Position.X - pos.X
		dz := tile.Position.Z - pos.Z
		dist := dx*dx + dz*dz

		if dist < minDist {
			minDist = dist
			closest = tile
		}
	}

	return closest
}

func (g *GameTerrainService) GetGlobalTilesAt(positions []objects.Vector3) []*objects.TerrainInfo {
	tiles := make([]*objects.TerrainInfo, len(positions))
	for i, pos := range positions {
		tiles[i] = g.GetGlobalTileAt(pos)
	}
	return tiles
}

func (g *GameTerrainService) PopulateTiles() {
	g.PopulateTilesFrom(g.terrainGen)
	g.PopulateTilesFrom(g.terrainGen1)
	g.PopulateTilesFrom(g.terrainGen2)
}

func (g *GameTerrainService) PopulateTilesFrom(terrainGen *objects.TerrainGen) {
	for _, info := range terrainGen.GetWorldInfo().TerrainInfo {
		info.Position = info.Position.Add(terrainGen.GlobalPos)
		info.PositionL = info.PositionI
		info.PositionI = objects.WorldToHexPosition(info.Position)
		if _, ok := g.Tiles[info.PositionI]; ok {
			fmt.Println("Duplicate tile at ", info.PositionI, "")
		}
		g.Tiles[info.PositionI] = info
	}
}
