using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
[Tool]
public partial class River : Resource, IFeature
{
    private WorldInfo worldInfo;
    private FeatureArgs args;
    private List<TerrainInfo> tiles = [];
    public void Set(WorldInfo worldInfo, FeatureArgs args)
    {
        this.worldInfo = worldInfo;
        this.args = args;
    }

    public void Generate()
    {
        var StartTile = worldInfo.EdgeTiles.OrderByDescending(tile => tile.TileHeight).First();
        if (StartTile.TileType != TileUtil.TileType.Grass)
        {
            StartTile = worldInfo.EdgeTiles.OrderByDescending(tile => tile.TileHeight).ToList()[1];
        }
        StartTile.TileType = TileUtil.TileType.River;
        var EndTile = worldInfo.EdgeTiles.OrderByDescending(tile => tile.Position.DistanceTo(StartTile.Position)).First();
        EndTile.TileType = TileUtil.TileType.River;
        var tile = StartTile;
        while (tile != EndTile)
        {
            tile.TileType = TileUtil.TileType.River;
            var availableNeighbours = tile.Neighbours.Where(n => !tiles.Contains(n) && n != null).ToList();
            if (availableNeighbours.Count == 0)
            {
                GD.PrintErr("River generation stuck - no available path");
                break;
            }
    
            var allowedNeighbours = availableNeighbours
                .Where(n => n.TileHeight >= tile.TileHeight - 1) // drop <= 1
                .Where(n => n.TileHeight <= tile.TileHeight)     // no uphill
                .ToList();

            if (allowedNeighbours.Count == 0)
            {
                GD.PrintErr("River generation stuck - no available path");
                break;
            }

            // Prefer flat tiles first, then lower ones
            var nextTile = allowedNeighbours
                .OrderByDescending(n => n.TileHeight) // higher first (tileHeight or tileHeight-1)
                .First();

            nextTile.Connected = true;
            if (nextTile.TileHeight > tile.TileHeight)
            {
                GD.PrintErr($"next tile is higher than current tile: {nextTile.TileHeight} > {tile.TileHeight}");
                nextTile.Connected = false;
                break;
            }
            tile.NextTile = nextTile;
            nextTile.PreviousTile = tile;
            tile = nextTile;
            tiles.Add(tile);
        }
        if (tile != EndTile)
        {
            EndTile.TileType = TileUtil.TileType.Grass;
        }

        GD.Print($"River generated from {StartTile.Position} {StartTile.TileHeight} to {EndTile.Position} {EndTile.TileHeight}");
        GD.Print($"Edge tiles: {worldInfo.EdgeTiles.Length}");
        GD.Print(tiles.Count);
    }

    public void Destroy()
    {
        foreach (var tile in tiles)
        {
            tile.TileType = TileUtil.TileType.Grass;
        }
        tiles.Clear();
    }

    public void Update()
    {
    }
}
