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
    public TileUtil.TileType DefaultTile;
    public FeatureArgs FeatureArgs;
    public int Radius;
    public int Amplitude;
    public int Features;
    public float hexSizeX;
    public float hexSizeY;
    public List<string> FeaturesArray = [];
    public List<IFeature> FeaturesList = [];
    public Dictionary<Vector2I, CornerInfo> Corners = new Dictionary<Vector2I, CornerInfo>();
    private static SplitMix64 random;
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
    
    public void GenerateTerrainInfo(FastNoiseLite noise, int seed)
    {
        random = new SplitMix64((ulong)seed);
        for (int q = -Radius; q <= Radius; q++)
        {
            int r1 = Math.Max(-Radius, -q - Radius);
            int r2 = Math.Min(Radius, -q + Radius);

            for (int r = r1; r <= r2; r++)
            {
                var tileInfo = new TerrainInfo()
                {
                    TileType = DefaultTile
                };
                tileInfo.Position = HexToWorldPosition(q, r);
                tileInfo.PositionI = new Vector2I(q, r);
                int row = q + Radius;

                tileInfo.z = tileInfo.PositionI.Y;
                Vector2I hexCoord = new Vector2I(q, r);
                
                var v = noise.GetNoise2D(tileInfo.Position.X, tileInfo.Position.Y) + 1;
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
        GenerateCorners();
        for (int i = 1; i <= Features; i++)
        {
            var feature = GetRandomFeature();
            feature.Set(this, FeatureArgs);
            feature.Generate(seed);
            FeaturesList.Add(feature);
        }
    }
    private List<TerrainInfo> GetHexNeighbors(int q, int r)
    {
        List<TerrainInfo> neighbors = new List<TerrainInfo>();
        
        foreach (var offset in HexNeighborOffsets)
        {
            Vector2I neighborCoord = new Vector2I(q + offset.X, r + offset.Y);
            neighbors.Add(TerrainInfo.GetValueOrDefault(neighborCoord));
        }
        
        return neighbors;
    }

    public void UpdateNeighbors()
    {
        foreach (var hex in TerrainInfo.Values)
        {
            List<TerrainInfo> neighbors = new List<TerrainInfo>();
            foreach (var offset in HexNeighborOffsets)
            {
                Vector2I neighborCoord = new Vector2I(hex.PositionI.X + offset.X, hex.PositionI.Y + offset.Y);
                var tile = Globals.GM.CurrentGameData.GetTileAt(neighborCoord);
                if (tile != null)
                {
                    neighbors.Add(tile);
                }
            }
            
            hex.Neighbours = neighbors.ToArray();
        }
    }
    private void GenerateCorners()
    {
        Corners.Clear();
        foreach (var terrainInfo in TerrainInfo.Values)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2I id = terrainInfo.PositionI * 3
                              + HexNeighborOffsets[i]
                              + HexNeighborOffsets[(i + 1) % 6];

                if (!Corners.TryGetValue(id, out var corner))
                {
                    float angle = Mathf.DegToRad(-60f * i);
                    corner = new CornerInfo
                    {
                        Id = id,
                        Position = terrainInfo.Position +
                                   new Vector2(hexSizeX * Mathf.Cos(angle), hexSizeY * Mathf.Sin(angle))
                    };
                    Corners[id] = corner;
                }

                corner.Tiles.Add(terrainInfo);
                terrainInfo.Corners[i] = corner;
            }
        }
    }
    // All tiles that share at least one corner with this tile (excluding itself)
    public List<TerrainInfo> GetTilesSharingCorner(TerrainInfo tile)
    {
        var result = new List<TerrainInfo>();
        foreach (var corner in tile.Corners)
        foreach (var other in corner.Tiles)
            if (other != tile)
                result.Add(other);
        return result;
    }

    // Corners two tiles have in common (2 if neighbours, otherwise 0)
    public List<CornerInfo> GetSharedCorners(TerrainInfo a, TerrainInfo b)
    {
        return a.Corners.Where(c => c.Tiles.Contains(b)).ToList();
    }

    public CornerInfo GetCorner(List<TerrainInfo> tiles)
    {
        foreach (var corner in Corners)
        {
            if (corner.Value.Tiles.Count == tiles.Count && corner.Value.Tiles.All(tiles.Contains))
                return corner.Value;
        }
        GD.Print("No corner found for tiles");
        return null;
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
    private Vector2 HexToWorldPosition(int q, int r)
    {
        
        float x = hexSizeX * (3.0f / 2.0f * q);
        float z = hexSizeY * (Mathf.Sqrt(3.0f) / 2.0f * q + Mathf.Sqrt(3.0f) * r);
        
        return new Vector2(x, z);
    }
    public IFeature GetRandomFeature()
    {
        // Get all types that implement IFeature
        var featureTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => typeof(IFeature).IsAssignableFrom(type) && 
                           !type.IsInterface && 
                           !type.IsAbstract && 
                           !FeaturesArray.Contains(type.Name))
            .ToList();
        
        if (featureTypes.Count == 0)
        {
            throw new InvalidOperationException("No implementations of IFeature found.");
        }
        
        // Select a random type
        var randomType = featureTypes[random.NextInt(featureTypes.Count)];
        FeaturesArray.Add(randomType.Name);
        // Create an instance of the random type
        return (IFeature)Activator.CreateInstance(randomType);
    }

}