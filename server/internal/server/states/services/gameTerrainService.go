package services

import (
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

// GetTileAt WRONG q is not sraight is sideways, use Vec3 pos as default instead, make a shared tiles dict in here for all and ref that
func (g *GameTerrainService) GetTileAt(pos objects.Vector2I) *objects.TerrainInfo {
	mainGen := g.terrainGen

	if pos.X < -mainGen.Radius {
		leftCenterX := -(mainGen.Radius + g.terrainGen1.Radius + 1)

		pos.X -= leftCenterX

		return g.terrainGen1.GetTileAt(pos)
	}

	if pos.X > mainGen.Radius {
		rightCenterX := mainGen.Radius + g.terrainGen2.Radius + 1

		pos.X -= rightCenterX

		return g.terrainGen2.GetTileAt(pos)
	}

	return mainGen.GetTileAt(pos)
}

func (g *GameTerrainService) GetTiles(positions []objects.Vector2I) []*objects.TerrainInfo {
	tiles := make([]*objects.TerrainInfo, len(positions))
	for i, pos := range positions {
		tiles[i] = g.GetTileAt(pos)
	}
	return tiles
}
func (g *GameTerrainService) GetGlobalTileAt(pos objects.Vector3) *objects.TerrainInfo {
	pos2D := objects.WorldToHexPosition(pos.X, pos.Z)
	return g.GetTileAt(pos2D)
}
func (g *GameTerrainService) GetGlobalTilesAt(positions []objects.Vector3) []*objects.TerrainInfo {
	tiles := make([]*objects.TerrainInfo, len(positions))
	for i, pos := range positions {
		tiles[i] = g.GetGlobalTileAt(pos)
	}
	return tiles
}

func (g *GameTerrainService) PopulateTiles() {
	for _, info := range g.terrainGen.GetWorldInfo().TerrainInfo {
		info.Position = info.Position.Add(g.terrainGen.GlobalPos)
		info.PositionL = info.PositionI
		info.PositionI = objects.WorldToHexPosition(info.Position.X, info.Position.Z)
		g.Tiles[info.PositionI] = info
	}
	for _, info := range g.terrainGen1.GetWorldInfo().TerrainInfo {
		info.Position = info.Position.Add(g.terrainGen1.GlobalPos)
		info.PositionL = info.PositionI
		info.PositionI = objects.WorldToHexPosition(info.Position.X, info.Position.Z)
		g.Tiles[info.PositionI] = info
	}
	for _, info := range g.terrainGen2.GetWorldInfo().TerrainInfo {
		info.Position = info.Position.Add(g.terrainGen2.GlobalPos)
		info.PositionL = info.PositionI
		info.PositionI = objects.WorldToHexPosition(info.Position.X, info.Position.Z)
		g.Tiles[info.PositionI] = info
	}
}
