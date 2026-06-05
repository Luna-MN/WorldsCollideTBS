using Godot;
using System;
using System.Collections.Generic;

[Tool]
[GlobalClass]
public partial class TerrainGen : Node3D
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
    public WorldInfo worldInfo;
    [ExportToolButton("Generate Terrain")] private Callable GenerateTerrainButton => Callable.From(_Ready);
    public int seed;

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
            FeatureArgs = FeatureArgs
        };
        worldInfo.GenerateTerrainInfo(noise, seed);
        GenerateTerrain(worldInfo);
    }
    private void GenerateTerrain(WorldInfo worldInfo)
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }
        foreach (TerrainInfo info in worldInfo.TerrainInfo.Values)
        {
            var hexInstance = TerrainScene.Instantiate<Tile>();
            AddChild(hexInstance);
            hexInstance.Position = info.Position;
            hexInstance.RotationDegrees = new Vector3(0, 90, 0);
            hexInstance.Set(TileMaps[info.TileType], TopTileMaps[info.TileTopType], info.Position.X, info.Position.Z, info);
            hexInstance.GenerateTile();
        }
    }
    
    private Vector3 HexToWorldPosition(int q, int r)
    {
        float hexSize = 1.15f;
        
        float x = hexSize * (3.0f / 2.0f * q);
        float z = hexSize * (Mathf.Sqrt(3.0f) / 2.0f * q + Mathf.Sqrt(3.0f) * r);
        
        return new Vector3(x, 0, z);
    }
    private Vector2I WorldPositionToHex(Vector3 worldPos)
    {
        float hexSize = 1.15f;
    
        float x = worldPos.X / hexSize;
        float z = worldPos.Z / hexSize;
    
        float q = (2.0f / 3.0f) * x;
        float r = (-1.0f / 3.0f) * x + (Mathf.Sqrt(3.0f) / 3.0f) * z;
    
        // Round to nearest hex coordinates
        int hexQ = Mathf.RoundToInt(q);
        int hexR = Mathf.RoundToInt(r);
    
        return new Vector2I(hexQ, hexR);
    }
    public TerrainInfo GetTileAt(Vector3 pos)
    {
        var pos2i = WorldPositionToHex(pos);
        return worldInfo.TerrainInfo[pos2i];
    }
    public TerrainInfo GetTileAt(Vector2I pos)
    {
        return worldInfo.TerrainInfo.GetValueOrDefault(pos);
    }
}