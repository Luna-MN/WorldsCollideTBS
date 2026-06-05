using Godot;

public class GameData
{
    public long Seed1;
    public long GameSeed;
    public long Seed2;
    public ulong ID1;
    public ulong ID2;
    public TerrainGen TerrainGen, TerrainGen1, TerrainGen2;
    
    public TerrainInfo GetTileAt(Vector2I pos)
    {
        var terrainGen = TerrainGen;
        if (pos.X < TerrainGen.Radius)
        {
            var leftCenterX = -(terrainGen.Radius + TerrainGen1.Radius + 1);
            terrainGen = TerrainGen1;
            pos -= new Vector2I(leftCenterX, 0);
        }   
        else if (pos.X > TerrainGen.Radius * 2)
        {
            var rightCenterX = (terrainGen.Radius + TerrainGen2.Radius + 1);
            terrainGen = TerrainGen2;
            pos -= new Vector2I(rightCenterX, 0);
        }
        return terrainGen.GetTileAt(pos);
    }

    public void UpdateTileNeighbours()
    {
        // TerrainGen.worldInfo.UpdateNeighbors();
        // TerrainGen1.worldInfo.UpdateNeighbors();
        // TerrainGen2.worldInfo.UpdateNeighbors();
    }
}