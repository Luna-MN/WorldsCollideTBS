using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Packets.Classes.Features;

[Tool]
public partial class River : Resource, IConnectionFeature
{
    private WorldInfo worldInfo;
    private FeatureArgs args;
    public FeatureType FeatureType => FeatureType.Connection;
    public List<CornerInfo> tiles { get; } = [];
    public void Set(WorldInfo worldInfo, FeatureArgs args)
    {
        this.worldInfo = worldInfo;
        this.args = args;
    }

    public void Generate(int seed)
    {
        var random = new SplitMix64((ulong)seed);

        // highest edge tile (random pick between ties)
        int maxHeight = worldInfo.EdgeTiles.Max(t => t.TileHeight);
        var highest = worldInfo.EdgeTiles.Where(t => t.TileHeight == maxHeight).ToList();
        var startTile = highest[random.NextInt(highest.Count)];

        // a corner of the start tile that sits on the map edge (touches fewer than 3 tiles)
        var edgeCorners = startTile.Corners.Where(c => c.Tiles.Count < 3).ToList();
        if (edgeCorners.Count == 0)
        {
            GD.PrintErr("River generation failed - start tile has no edge corner");
            return;
        }
        var corner = edgeCorners[random.NextInt(edgeCorners.Count)];

        var endTile = worldInfo.TerrainInfo.Values
            .OrderByDescending(t => t.Position.DistanceTo(startTile.Position))
            .First();

        var visited = new HashSet<CornerInfo>();
        while (true)
        {
            visited.Add(corner);
            corner.terrainConnection = TileUtil.TileConnection.River;
            tiles.Add(corner);

            if (corner.Tiles.Contains(endTile))
                break;

            int height = CornerHeight(corner);
            var next = GetNeighbourCorners(corner)
                .Where(c => !visited.Contains(c) && CornerHeight(c) <= height)
                .OrderBy(c => c.Position.DistanceTo(endTile.Position))
                .FirstOrDefault();

            if (next == null)
            {
                GD.Print("River generation stuck - no downhill or flat corner left");
                break;
            }
            corner = next;
        }
        GD.Print($"River generated from {startTile.Position} ({startTile.TileHeight}) towards {endTile.Position} ({endTile.TileHeight}), {tiles.Count} corners");
    }

    // A corner sits as low as the lowest tile touching it
    private static int CornerHeight(CornerInfo corner)
    {
        return corner.Tiles.Min(t => t.TileHeight);
    }

    // Corners connected to this one by a hex edge
    private static IEnumerable<CornerInfo> GetNeighbourCorners(CornerInfo corner)
    {
        var result = new HashSet<CornerInfo>();
        foreach (var tile in corner.Tiles)
        {
            int i = Array.IndexOf(tile.Corners, corner);
            result.Add(tile.Corners[(i + 1) % 6]);
            result.Add(tile.Corners[(i + 5) % 6]);
        }
        return result;
    }

    public void Destroy()
    {
        foreach (var tile in tiles)
        {
            tile.terrainConnection = TileUtil.TileConnection.defaultConnection;
        }
        tiles.Clear();
    }

    public void Update()
    {
    }
}
