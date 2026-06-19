using System.Linq;
using System.Text.Json.Serialization;
using Packets;

public class GameDataJSON
{
    public FactionDataJSON[] factions;
    public ArmyDataJSON[] armies;
    public UnitDataJSON[] units;
    
    [JsonConstructor]
    public GameDataJSON() { }
    public GameDataJSON(FactionDataJSON[] factions, ArmyDataJSON[] armies, UnitDataJSON[] units)
    {
        this.factions = factions;
        this.armies = armies;
        this.units = units;
    }

}

public class FactionDataJSON
{
    public long ID;
    public string Name;
    public string Description;
    public long[] armyIds;
    
    [JsonConstructor]
    public FactionDataJSON() { }
    public FactionDataJSON(FactionData faction)
    {
        ID = faction.Id;
        Name = faction.Name;
        Description = faction.Description;
        armyIds = faction.ArmyIds.ToArray();
    }
}

public class ArmyDataJSON
{
    public long ID;
    public string Name;
    public string Description;
    public long[] unitIds;
    [JsonConstructor]
    public ArmyDataJSON(){}
    public ArmyDataJSON(ArmyData army)
    {
        ID = army.Id;
        Name = army.Name;
        Description = army.Description;
        unitIds = army.UnitIds.ToArray();
    }
}

public class UnitDataJSON
{
    public long ID;
    public string Name;
    public string Attacks;
    public string Movement;
    public long MaxHP;
    public long AP;
    public long speed;
    public long factionId;
    [JsonConstructor]
    public UnitDataJSON(){}
    public UnitDataJSON(Packets.UnitData unit)
    {
        ID = unit.Id;
        Name = unit.Name;
        Attacks = unit.Attacks;
        Movement = unit.Movement;
        MaxHP = unit.MaxHP;
        AP = unit.AP;
        speed = (long)unit.Speed;
        factionId = unit.FactionId;
    }
}