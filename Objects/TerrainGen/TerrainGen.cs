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
    [Export(PropertyHint.Range, "1,3")] 
    private int Features;
    [Export]
    private TileMapController TileMaps;
    [Export]
    private FastNoiseLite noise;
    private WorldInfo worldInfo;
    [ExportToolButton("Generate Terrain")] private Callable GenerateTerrainButton => Callable.From(Generate);
    

    public override void _Ready()
    {
        Generate();
    }

    private void Generate()
    {
        worldInfo = new WorldInfo()
        {
            Radius = Radius,
            Amplitude = Amplitude,
            Features = Features
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
            hexInstance.Set(TileMaps[info.TileType], info.Position.X, info.Position.Z, info);

            hexInstance.GenerateTile();
        }
    }
    
    private void GenerateTerrain() // legacy, leaving here for a bit
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }
        
        for (int q = -Radius; q <= Radius; q++)
        {
            int r1 = Math.Max(-Radius, -q - Radius);
            int r2 = Math.Min(Radius, -q + Radius);
            
            for (int r = r1; r <= r2; r++)
            {
                var hexInstance = TerrainScene.Instantiate<Tile>();
                AddChild(hexInstance);
                
                var worldPos = HexToWorldPosition(q, r);
                hexInstance.Position = worldPos;
                hexInstance.RotationDegrees = new Vector3(0, 90, 0);
                hexInstance.Set(TileMaps[TileUtil.TileType.Grass], worldPos.X, worldPos.Z);
                var v = noise.GetNoise2D(worldPos.X, worldPos.Z) + 1;
                float scaledValue = v * Amplitude;
                int val = (int)Math.Round(scaledValue * 2) / 2;

                hexInstance.GenerateTile();
            }
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