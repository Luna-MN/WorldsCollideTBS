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
}

func NewGameTerrainService(seed1, seed2, mainSeed uint64) GameTerrainService {
	return GameTerrainService{
		seed1:    seed1,
		seed2:    seed2,
		mainSeed: mainSeed,
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
}
