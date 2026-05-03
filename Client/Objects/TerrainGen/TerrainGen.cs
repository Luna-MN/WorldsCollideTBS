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
    private int Radius;
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
    private WorldInfo worldInfo;
    [ExportToolButton("Generate Terrain")] private Callable GenerateTerrainButton => Callable.From(Generate);
    

    public override void _Ready()
    {
        TileMaps.GDBackup();
        TopTileMaps.GDBackup();
        Generate();
    }

    private void Generate()
    {
        worldInfo = new WorldInfo()
        {
            Radius = Radius,
            Amplitude = Amplitude,
            Features = Features,
            DefaultTile = DefaultTile,
            FeatureArgs = FeatureArgs
        };
        worldInfo.GenerateTerrainInfo(noise);
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
}