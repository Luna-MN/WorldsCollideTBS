using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
[Tool]
public partial class River : Resource, IFeature
{
    private WorldInfo worldInfo;
    private FeatureArgs args;
    private List<TileConnection> tiles = [];
    public void Set(WorldInfo worldInfo, FeatureArgs args)
    {
        this.worldInfo = worldInfo;
        this.args = args;
    }

    public void Generate(int seed)
    {
        var StartTile = worldInfo.EdgeTiles.OrderByDescending(tile => tile.TileHeight).ToList().First(x => x.Neighbours.OrderBy(n => n.TileHeight).First().TileHeight == x.TileHeight);
        if (StartTile.TileType != TileUtil.TileType.Grass)
        {
            StartTile = worldInfo.EdgeTiles.OrderByDescending(tile => tile.TileHeight).ToList().First(x => x.Neighbours.OrderBy(n => n.TileHeight).First().TileHeight == x.TileHeight);
        }

        var connectedTile = StartTile.Neighbours.OrderBy(n => n.TileHeight).First();
        StartTile.Connections[connectedTile] = TileUtil.TileConnection.River;
        var EndTile = worldInfo.EdgeTiles.OrderByDescending(tile => tile.Position.DistanceTo(StartTile.Position)).First();
        var tile = StartTile;
        while (tile != EndTile)
        {
            var availableNeighbours = tile.Neighbours.ToList();
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
                nextTile.Connected = true;
                break;
            }
            tile.NextTile = nextTile;
            nextTile.PreviousTile = tile;
            var newConnectedTile = connectedTile;
            // take the connected tile and see if it neighbors this tile as well
            if (nextTile.Neighbours.Contains(newConnectedTile))
            {
                // take that and make the connection type between the two tiles
                nextTile.Connections[newConnectedTile] = TileUtil.TileConnection.River;
            }
            else
            {
                // if it doesn't neighbor the connected tile, find the new connected tile that neighbors this tile, and the current tile
                newConnectedTile = tile.Neighbours.ToList().First(n => n.Neighbours.Contains(nextTile));
                // set that connection to be a river
                nextTile.Connections[newConnectedTile] = TileUtil.TileConnection.River;
            }
            // reset for the next round
            tiles.Add(new TileConnection()
            {
                terrainInfoMain = tile, 
                terrainInfoConnection = newConnectedTile,
                terrainInfoPrevConnection = connectedTile,
                terrainConnection = TileUtil.TileConnection.River
            });
            tile = nextTile;
            connectedTile = newConnectedTile;
        }
        GD.Print($"River generated from {StartTile.Position} {StartTile.TileHeight} to {EndTile.Position} {EndTile.TileHeight}");
        GD.Print($"Edge tiles: {worldInfo.EdgeTiles.Length}");
        GD.Print(tiles.Count);
    }

    public void Destroy()
    {
        foreach (var tile in tiles)
        {
            tile.terrainInfoMain.Connections[tile.terrainInfoConnection] = TileUtil.TileConnection.defaultConnection;
            tile.terrainInfoConnection.Connections[tile.terrainInfoMain] = TileUtil.TileConnection.defaultConnection;
            tile.terrainConnection = TileUtil.TileConnection.defaultConnection;
        }
        tiles.Clear();
    }

    public void Update()
    {
    }
}
