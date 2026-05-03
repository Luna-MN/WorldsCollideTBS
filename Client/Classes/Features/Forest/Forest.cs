using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Forest : Resource, IFeature
{
    private WorldInfo worldInfo;
    private FeatureArgs args;
    private FastNoiseLite noise;
    private float threshold;
    private List<TerrainInfo> tiles = [];
    public void Set(WorldInfo worldInfo, FeatureArgs args)
    {
        this.worldInfo = worldInfo;
        this.args = args;
        noise = args.ForestNoise;
        threshold = args.Threshold;
    }

    public void Generate()
    {
        foreach (var tile in worldInfo.TerrainInfo.Values)
        {
            var v = noise.GetNoise2D(tile.PositionI.X, tile.PositionI.Y);
            if (v > threshold)
            {
                tile.TileTopType = TileUtil.TileTopType.Tree;
            }
        }
        foreach (var tile in worldInfo.TerrainInfo.Values)
        {
            if (tile.TileTopType == TileUtil.TileTopType.Tree)
            {
                var trees = tile.Neighbours.Count(n => n != null && n.TileTopType == TileUtil.TileTopType.Tree);
                if (trees >= 3)
                {
                    tile.TopTileState = TileUtil.TileTopState.Medium;
                }
                if (trees >= 4)
                {
                    tile.TopTileState = TileUtil.TileTopState.Large;
                }
            }
        }
    }

    public void Destroy()
    {
    }

    public void Update()
    {

    }
}
