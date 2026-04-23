using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
[Tool]
public partial class River : Resource, IFeature
{
    private WorldInfo worldInfo;
    private List<TerrainInfo> tiles = [];
    public void Set(WorldInfo worldInfo)
    {
        this.worldInfo = worldInfo;
    }

    public void Generate()
    {
        var StartTile = worldInfo.EdgeTiles.OrderByDescending(tile => tile.TileHeight).First();
        StartTile.TileType = TileUtil.TileType.River;
        var EndTile = worldInfo.EdgeTiles.OrderBy(tile => tile.TileHeight).First();
        EndTile.TileType = TileUtil.TileType.River;
        EndTile.Connected = true;
        var tile = StartTile;
        while (tile != EndTile)
        {
            tile.TileType = TileUtil.TileType.River;
            tile.Connected = true;
            var availableNeighbours = tile.Neighbours.Where(n => !tiles.Contains(n)).ToList();
            if (availableNeighbours.Count == 0)
            {
                GD.PrintErr("River generation stuck - no available path");
                break;
            }
    
            // Choose the neighbor that goes downhill (toward EndTile)
            var nextTile = availableNeighbours.OrderBy(neighbour => neighbour.TileHeight).First();
            if (nextTile.TileHeight > tile.TileHeight)
            {
                GD.PrintErr($"next tile is higher than current tile: {nextTile.TileHeight} > {tile.TileHeight}");
                break;
            }
            tile.NextTile = nextTile;
            nextTile.PreviousTile = tile;
            tile = nextTile;
            tiles.Add(tile);
        }

        if (tile != EndTile)
        {
            EndTile.Connected = false;
            EndTile.TileType = TileUtil.TileType.Grass;
        }

        GD.Print($"River generated from {StartTile.Position} {StartTile.TileHeight} to {EndTile.Position} {EndTile.TileHeight}");
        GD.Print($"Edge tiles: {worldInfo.EdgeTiles.Length}");
        GD.Print(tiles.Count);
    }
    public void Destroy() { }
    public void Update() { }
}
