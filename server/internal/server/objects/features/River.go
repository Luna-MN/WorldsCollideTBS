package features

import (
	"fmt"
	"server/internal/server/objects"
	"server/internal/server/objects/tiles"
	"sort"
)

type River struct {
	WorldInfo *tiles.WorldInfo
	args      *tiles.FeatureArgs
	tiles     []*tiles.TerrainInfo
}

func (r *River) Set(world *tiles.WorldInfo, args *tiles.FeatureArgs) {
	r.WorldInfo = world
	r.args = args
	r.tiles = make([]*tiles.TerrainInfo, 0)
}

func (r *River) Generate(seed uint64) {
	if r.WorldInfo == nil || len(r.WorldInfo.EdgeTiles) == 0 {
		fmt.Println("River generation failed - missing world info or edge tiles")
		return
	}

	edgeTiles := make([]*tiles.TerrainInfo, len(r.WorldInfo.EdgeTiles))
	copy(edgeTiles, r.WorldInfo.EdgeTiles)

	sort.Slice(edgeTiles, func(i, j int) bool {
		return edgeTiles[i].TileHeight > edgeTiles[j].TileHeight
	})

	startTile := edgeTiles[0]
	if startTile.TileType != objects.Grass {
		if len(edgeTiles) < 2 {
			fmt.Println("River generation failed - no valid start tile")
			return
		}

		startTile = edgeTiles[1]
	}

	startTile.TileType = objects.River
	r.setTerrainTile(startTile)

	endTile := r.furthestEdgeTileFrom(startTile)
	endTile.TileType = objects.River
	r.setTerrainTile(endTile)

	tile := startTile

	for tile.PositionI != endTile.PositionI {
		tile.TileType = objects.River
		r.setTerrainTile(tile)

		availableNeighbours := make([]*tiles.TerrainInfo, 0)

		for _, neighbour := range tile.Neighbors {
			if !r.containsTile(neighbour) {
				availableNeighbours = append(availableNeighbours, neighbour)
			}
		}

		if len(availableNeighbours) == 0 {
			fmt.Println("River generation stuck - no available path")
			break
		}

		allowedNeighbours := make([]*tiles.TerrainInfo, 0)

		for _, neighbour := range availableNeighbours {
			if neighbour.TileHeight >= tile.TileHeight-1 && neighbour.TileHeight <= tile.TileHeight && neighbour.TileType != objects.River {
				allowedNeighbours = append(allowedNeighbours, neighbour)
			}
		}

		if len(allowedNeighbours) == 0 {
			fmt.Println("River generation stuck - no available path")
			break
		}

		sort.Slice(allowedNeighbours, func(i, j int) bool {
			return allowedNeighbours[i].TileHeight > allowedNeighbours[j].TileHeight
		})

		nextTile := allowedNeighbours[0]

		if nextTile.TileHeight > tile.TileHeight {
			fmt.Printf(
				"next tile is higher than current tile: %d > %d\n",
				nextTile.TileHeight,
				tile.TileHeight,
			)
			break
		}

		// TerrainInfo currently has no Connected, NextTile, or PreviousTile fields.
		// Add those fields to objects.TerrainInfo if you still need that behavior.

		tile = nextTile
		r.tiles = append(r.tiles, tile)
	}

	if tile.PositionI != endTile.PositionI {
		endTile.TileType = objects.Grass
		r.setTerrainTile(endTile)
	}
}

func (r *River) furthestEdgeTileFrom(startTile *tiles.TerrainInfo) *tiles.TerrainInfo {
	endTile := r.WorldInfo.EdgeTiles[0]
	maxDistance := startTile.Position.DistanceTo(&endTile.Position)

	for _, tile := range r.WorldInfo.EdgeTiles[1:] {
		distance := startTile.Position.DistanceTo(&tile.Position)
		if distance > maxDistance {
			maxDistance = distance
			endTile = tile
		}
	}

	return endTile
}

func (r *River) containsTile(tile *tiles.TerrainInfo) bool {
	for _, existingTile := range r.tiles {
		if existingTile.PositionI == tile.PositionI {
			return true
		}
	}

	return false
}
func (r *River) setTerrainTile(tile *tiles.TerrainInfo) {
	r.WorldInfo.TerrainInfo[tile.PositionI] = tile
}
func (r *River) Destroy() {
}

func (r *River) Update() {
}
