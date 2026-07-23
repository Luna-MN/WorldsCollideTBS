using System.Collections.Generic;
using System.Linq;
using Godot;
using Packets;

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
    public Dictionary<int, IUnit> MyUnits = new Dictionary<int, IUnit>();
    public List<UnitData> MyUnitData = new List<UnitData>();
    public Color MyColor = Colors.White;
    public Color MyDefaultColor = Colors.Blue;
    public long EnemyArmyID;
    public Dictionary<int, IUnit> EnemyUnits = new Dictionary<int, IUnit> ();
    public Color EnemyColor = Colors.White;
    public Color EnemyDefaultColor = Colors.Red;
    public Dictionary<Vector2I, TerrainInfo> Tiles = new Dictionary<Vector2I, TerrainInfo>();
    public TerrainGen TerrainGen, TerrainGen1, TerrainGen2;
    public Side MySide;
    private int currUnitID = 1;
    
    public ulong[] TurnOrder;
    public int currTurnPointer;
    public bool MyTurn;
    private int currSkillID;
    public int CurrentSkillID
    {
        get
        {
            currSkillID++;
            return currSkillID;
        }
    }

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
    
    public List<UnitData> InitUnitData(List<UnitDataJSON> units, ulong id)
    {
        var dataList = new List<UnitData>();
        var army = Globals.GDH.GetArmy(MyArmyID);
        foreach (var unit in units)
        {
            for (var i = 0; i < army.unitIds.First(x => x.unitId == unit.ID).count; i++)
            {
                var data = new UnitData(unit, id, currUnitID);
                currUnitID++;
                dataList.Add(data);    
            }
        }
        return dataList;
    }

    public void InitEnemyArmy(PackedScene unitScene, UnitIDsMessage enemyIDs)
    {
        var units = GetOpponentUnits();
        Dictionary<long, List<int>> unitIDs = new Dictionary<long, List<int>>(); // JSONID, current game ID
        foreach (var ids in enemyIDs.Ids)
        {
            if (!unitIDs.ContainsKey(ids.Id))
            {
                unitIDs.Add(ids.Id, new List<int>() { ids.UnitId });
            }
            else
            {
                unitIDs[ids.Id].Add(ids.UnitId);
            }
        }
        foreach (var unit in units.Values)
        {
            foreach (var id in unitIDs[unit.ID])
            {
                var unitNode = unitScene.Instantiate<UniversalUnit>();
                unitNode.Color = EnemyColor == Colors.White ? EnemyDefaultColor : EnemyColor;
                unitNode.Data = new UnitData(unit, EnemyID, id);
                EnemyUnits[id] = unitNode;
            }

        }
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