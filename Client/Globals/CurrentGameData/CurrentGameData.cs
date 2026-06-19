using System.Collections.Generic;
using Godot;

public class CurrentGameData
{
    public enum Side
    {
        left,
        right
    }
    public long LeftSeed;
    public long GameSeed;
    public long RightSeed;
    public ulong EnemyID;
    public long MyArmyID;
    public long EnemyArmyID;
    public Dictionary<Vector2I, TerrainInfo> Tiles = new Dictionary<Vector2I, TerrainInfo>();
    public TerrainGen TerrainGen, TerrainGen1, TerrainGen2;
    public Side MySide;

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

    private void PopulateTiles()
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

    public ArmyDataJSON GetMyArmy()
    {
        return Globals.GDH.GetArmy(MyArmyID);
    }

    public ArmyDataJSON GetOpponentArmy()
    {
        return Globals.GDH.GetArmy(EnemyArmyID);
    }

    public Dictionary<long, UnitDataJSON> GetMyUnits()
    {
        return Globals.GDH.GetUnitsForArmy(MyArmyID);
    }
    
    public Dictionary<long, UnitDataJSON> GetOpponentUnits()
    {
        return Globals.GDH.GetUnitsForArmy(EnemyArmyID);
    }

}