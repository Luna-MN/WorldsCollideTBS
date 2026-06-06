using System.Collections.Generic;
using Godot;

public class GameData
{
    public long Seed1;
    public long GameSeed;
    public long Seed2;
    public ulong ID1;
    public ulong ID2;
    public Dictionary<Vector2I, TerrainInfo> Tiles = new Dictionary<Vector2I, TerrainInfo>();
    public TerrainGen TerrainGen, TerrainGen1, TerrainGen2;

    public TerrainInfo GetTileAt(Vector2I pos)
    {
        return Tiles.GetValueOrDefault(pos);
    }

    public void UpdateTileNeighbours()
    {
        PopulateTiles();
        TerrainGen.worldInfo.UpdateNeighbors();
        TerrainGen1.worldInfo.UpdateNeighbors();
        TerrainGen2.worldInfo.UpdateNeighbors();
    }

    public void PopulateTiles()
    {
        foreach (var tileKVP in TerrainGen.worldInfo.TerrainInfo)
        {
            var tile = tileKVP.Value;
            tile.Position = TerrainGen.GlobalPosition + tile.Position;
            tile.PositionL = tile.PositionI;
            tile.PositionI = TerrainGen.WorldPositionToHex(tile.Position);
            Tiles[tile.PositionI] = tile;
        }

        foreach (var tileKVP in TerrainGen1.worldInfo.TerrainInfo)
        {
            var tile = tileKVP.Value;
            tile.Position = TerrainGen1.GlobalPosition + tile.Position;
            tile.PositionL = tile.PositionI;
            tile.PositionI = TerrainGen1.WorldPositionToHex(tile.Position);
            Tiles[tile.PositionI] = tile;
        }

        foreach (var tileKVP in TerrainGen2.worldInfo.TerrainInfo)
        {
            var tile = tileKVP.Value;
            tile.Position = TerrainGen2.GlobalPosition + tile.Position;
            tile.PositionL = tile.PositionI;
            tile.PositionI = TerrainGen2.WorldPositionToHex(tile.Position);
            Tiles[tile.PositionI] = tile;
        }

        GD.Print(TerrainGen.worldInfo.TerrainInfo.Count + TerrainGen1.worldInfo.TerrainInfo.Count +  TerrainGen2.worldInfo.TerrainInfo.Count);
        GD.Print(Tiles.Count);
    }

}