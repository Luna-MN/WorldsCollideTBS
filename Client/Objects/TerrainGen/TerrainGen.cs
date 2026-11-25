using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Packets.Classes.Features;
using Packets.Classes.Tiles;

[Tool]
[GlobalClass]
public partial class TerrainGen : Node2D
{
    [Export]
    private PackedScene TerrainScene;
    [Export] 
    public int Radius;
    [Export] 
    private int Amplitude;
    [Export(PropertyHint.Range, "0,3")] 
    private int Features;
    [Export]
    private TileMapController TileMaps;
    [Export]
    private FastNoiseLite noise;
    [Export]
    private TileUtil.TileType DefaultTile;
    [Export]
    public FeatureArgs FeatureArgs;
    [Export]
    public TopTileMapController TopTileMaps;
    [Export]
    public ConnectionMapController ConnectionMaps;
    public WorldInfo worldInfo;
    [ExportToolButton("Generate Terrain")] private Callable GenerateTerrainButton => Callable.From(_Ready);
    public int seed;
    [Export] public float HexSizeX;
    [Export] public float HexSizeY;
    [Export] public float OrgHeightOffset;
    [Export] public float HeightOffset;
    [Export] public int Zbonus;
    [Export] public int ZNeg;
    public override void _Ready()
    {
        TileMaps.GDBackup();
        if (Features != 0)
        {
            TopTileMaps.GDBackup();
        }
        Generate();
    }

    private void Generate()
    {
        if (seed != 0)
        {
            noise.SetSeed(seed);
        }
        worldInfo = new WorldInfo()
        {
            Radius = Radius,
            Amplitude = Amplitude,
            Features = Features,
            DefaultTile = DefaultTile,
            FeatureArgs = FeatureArgs,
            hexSizeX = HexSizeX,
            hexSizeY = HexSizeY
        };
        worldInfo.GenerateTerrainInfo(noise, seed);
        GenerateTerrain(worldInfo);
    }
    private void GenerateTerrain(WorldInfo worldInfo)
    {
        YSortEnabled = true;
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }

        int maxTileHeight = 0;
        foreach (TerrainInfo info in worldInfo.TerrainInfo.Values)
        {
            maxTileHeight = Math.Max(maxTileHeight, info.TileHeight);
        }

        int tileStride = maxTileHeight + 1;

        var sortedTerrain = worldInfo.TerrainInfo.Values
            .OrderBy(info => info.Position.Y)
            .ToList();

        Dictionary<float, int> rowIndexes = new();
        int nextRowIndex = 0;

        foreach (TerrainInfo info in sortedTerrain)
        {
            float rowKey = Mathf.Round(info.Position.Y);

            if (!rowIndexes.ContainsKey(rowKey))
            {
                rowIndexes[rowKey] = nextRowIndex;
                nextRowIndex++;
            }
        }

        int rowCount = nextRowIndex;

        int tileBandStart = ZNeg;
        int connectorBandStart = tileBandStart + rowCount * tileStride;
        int topTileBandStart = connectorBandStart + rowCount;

        foreach (TerrainInfo info in sortedTerrain)
        {
            float rowKey = Mathf.Round(info.Position.Y);
            int rowIndex = rowIndexes[rowKey];

            var hexInstance = TerrainScene.Instantiate<Tile>();
            AddChild(hexInstance);
            hexInstance.Position = info.Position;
            hexInstance.ZNeg = ZNeg;

            // Z bands:
            // tiles      -> tileBandStart
            // connectors -> connectorBandStart
            // top tiles  -> topTileBandStart
            hexInstance.BaseZIndex = tileBandStart + rowIndex * tileStride;
            hexInstance.ConnectorZIndex = connectorBandStart + rowIndex;
            hexInstance.TopTileZIndex = topTileBandStart + rowIndex;

            hexInstance.OrgHeightOffset = OrgHeightOffset;
            hexInstance.HeightOffset = HeightOffset;
            hexInstance.Owner = this;
            hexInstance.Set(TileMaps[info.TileType], TopTileMaps[info.TileTopType], info.Position.X, info.Position.Y, info);
            hexInstance.GenerateTile();
            info.tile = hexInstance;
        }

        foreach (IFeature feature in worldInfo.FeaturesList)
        {
            if (feature.FeatureType != FeatureType.Connection)
            {
                continue;
            }
            var conFeature = (IConnectionFeature)feature;
            for (int i = 0; i < conFeature.tiles.Count; i++)
            {
                var connection = conFeature.tiles[i];
                var map = ConnectionMaps[connection.terrainConnection];
                var prevConn = i > 0 ? conFeature.tiles[i - 1] : default;
                var nextConn = i + 1 < conFeature.tiles.Count ? conFeature.tiles[i + 1] : default;
                var state = connection.GetConnectionState([prevConn, nextConn]);
                var node = map[state].TileNode.Instantiate<Node2D>();

                int connectorZIndex = int.MinValue;
                float sortY = float.MinValue;

                foreach (var t in connection.Tiles)
                {
                    if (t.tile == null)
                    {
                        continue;
                    }

                    connectorZIndex = Math.Max(connectorZIndex, t.tile.ConnectorZIndex);
                    sortY = Math.Max(sortY, t.Position.Y);
                }

                if (connectorZIndex == int.MinValue)
                {
                    connectorZIndex = connectorBandStart;
                    sortY = connection.Position.Y;
                }

                var holder = new Node2D
                {
                    Position = new Vector2(connection.Position.X, sortY + 0.01f),
                    ZIndex = connectorZIndex
                };
                AddChild(holder);
                holder.AddChild(node);

                node.Position = new Vector2(0, connection.Position.Y - holder.Position.Y - GetTopHeightOffset(connection));
                GD.Print($"[{i}] state={state} corner={connection.Position} offset={GetTopHeightOffset(connection)} " +
                         $"tiles=[{string.Join(", ", connection.Tiles.Select(t => $"{t.PositionI}@h{t.TileHeight}"))}] " +
                         $"global={node.GlobalPosition}");
            }
        }
    }

    private float GetTopHeightOffset(CornerInfo corner)
    {
        int height = 0;
        foreach (var tile in corner.Tiles)
            height = Math.Max(height, tile.TileHeight);
        return height == 0 ? 0 : OrgHeightOffset + HeightOffset * (height - 1);
    }

    public Vector2 ClosestPoint(List<Vector2> points)
    {
        var calc = new Vector2();
        foreach (Vector2 point in points)
        {
            calc += point;
        }
        return calc / points.Count;
    }
    public Vector3 HexToWorldPosition(int q, int r)
    {
        
        float x = HexSizeX * (3.0f / 2.0f * q);
        float z = HexSizeY * (Mathf.Sqrt(3.0f) / 2.0f * q + Mathf.Sqrt(3.0f) * r);
        
        return new Vector3(x, 0, z);
    }
    public Vector2I WorldPositionToHex(Vector2 worldPos)
    {
        float hexSize = 1.15f;
    
        float x = worldPos.X / hexSize;
        float z = worldPos.Y / hexSize;
    
        float q = (2.0f / 3.0f) * x;
        float r = (-1.0f / 3.0f) * x + (Mathf.Sqrt(3.0f) / 3.0f) * z;
    
        // Round to nearest hex coordinates
        int hexQ = Mathf.RoundToInt(q);
        int hexR = Mathf.RoundToInt(r);
    
        return new Vector2I(hexQ, hexR);
    }
    public TerrainInfo GetTileAt(Vector2 pos)
    {
        var pos2i = WorldPositionToHex(pos);
        return worldInfo.TerrainInfo[pos2i];
    }
    public TerrainInfo GetTileAt(Vector2I pos)
    {
        return worldInfo.TerrainInfo.GetValueOrDefault(pos);
    }
}