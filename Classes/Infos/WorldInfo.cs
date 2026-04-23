using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
[Tool]
public class WorldInfo
{
    public Dictionary<Vector2I, TerrainInfo> TerrainInfo = new Dictionary<Vector2I, TerrainInfo>();
    public TerrainInfo[] EdgeTiles;
    public int Radius;
    public int Amplitude;
    public int Features;
    private static readonly Random random = new Random();
    public WorldInfo()
    {
    }
    private static readonly Vector2I[] HexNeighborOffsets = 
    {
        new Vector2I(1, 0),   
        new Vector2I(1, -1),  
        new Vector2I(0, -1),  
        new Vector2I(-1, 0),  
        new Vector2I(-1, 1),  
        new Vector2I(0, 1)    
    };
    
    public void GenerateTerrainInfo(FastNoiseLite noise)
    {
        for (int q = -Radius; q <= Radius; q++)
        {
            int r1 = Math.Max(-Radius, -q - Radius);
            int r2 = Math.Min(Radius, -q + Radius);

            for (int r = r1; r <= r2; r++)
            {
                var tileInfo = new TerrainInfo();
                tileInfo.Position = HexToWorldPosition(q, r);
                Vector2I hexCoord = new Vector2I(q, r);
                
                var v = noise.GetNoise2D(tileInfo.Position.X, tileInfo.Position.Z) + 1;
                float scaledValue = v * Amplitude;
                int val = (int)Math.Round(scaledValue * 2) / 2;
                tileInfo.TileHeight = val;
                TerrainInfo[hexCoord] = tileInfo;
            }
        }
        foreach (var kvp in TerrainInfo)
        {
            var coord = kvp.Key;
            var terrainInfo = kvp.Value;
            
            var neighbors = GetHexNeighbors(coord.X, coord.Y);
            terrainInfo.Neighbours = neighbors.ToArray();
        }
        EdgeTiles = GetAllEdgeTiles().ToArray();
        for (int i = 0; i <= Features; i++)
        {
            var feature = GetRandomFeature();
            feature.Set(this);
            feature.Generate();
        }
    }
    private List<TerrainInfo> GetHexNeighbors(int q, int r)
    {
        List<TerrainInfo> neighbors = new List<TerrainInfo>();
        
        foreach (var offset in HexNeighborOffsets)
        {
            Vector2I neighborCoord = new Vector2I(q + offset.X, r + offset.Y);
            
            if (TerrainInfo.TryGetValue(neighborCoord, out TerrainInfo neighbor))
            {
                neighbors.Add(neighbor);
            }
        }
        
        return neighbors;
    }
    public List<Vector2I> GetHexNeighborCoords(int q, int r)
    {
        List<Vector2I> neighborCoords = new List<Vector2I>();
        
        foreach (var offset in HexNeighborOffsets)
        {
            Vector2I neighborCoord = new Vector2I(q + offset.X, r + offset.Y);
            
            if (IsValidHexCoordinate(neighborCoord.X, neighborCoord.Y))
            {
                neighborCoords.Add(neighborCoord);
            }
        }
        
        return neighborCoords;
    }
    private bool IsValidHexCoordinate(int q, int r)
    {
        // Check if the coordinate is within the hexagonal radius
        return Math.Abs(q) <= Radius && 
               Math.Abs(r) <= Radius && 
               Math.Abs(q + r) <= Radius;
    }

    public List<TerrainInfo> GetAllEdgeTiles()
    {
        List<TerrainInfo> edgeTiles = new List<TerrainInfo>();
        
        foreach (var kvp in TerrainInfo)
        {
            var coord = kvp.Key;
            var terrainInfo = kvp.Value;
            
            if (IsEdgeTile(coord.X, coord.Y))
            {
                edgeTiles.Add(terrainInfo);
            }
        }
        
        return edgeTiles;
    }
    public bool IsEdgeTile(int q, int r)
    {
        // A tile is on the edge if it has fewer than 6 neighbors
        var neighbors = GetHexNeighborCoords(q, r);
        return neighbors.Count < 6;
    }
    private Vector3 HexToWorldPosition(int q, int r)
    {
        float hexSize = 1.15f;
        
        float x = hexSize * (3.0f / 2.0f * q);
        float z = hexSize * (Mathf.Sqrt(3.0f) / 2.0f * q + Mathf.Sqrt(3.0f) * r);
        
        return new Vector3(x, 0, z);
    }
    public static IFeature GetRandomFeature()
    {
        // Get all types that implement IFeature
        var featureTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => typeof(IFeature).IsAssignableFrom(type) && 
                           !type.IsInterface && 
                           !type.IsAbstract)
            .ToList();
        
        if (featureTypes.Count == 0)
        {
            throw new InvalidOperationException("No implementations of IFeature found.");
        }
        
        // Select a random type
        var randomType = featureTypes[random.Next(featureTypes.Count)];
        
        // Create an instance of the random type
        return (IFeature)Activator.CreateInstance(randomType);
    }

}