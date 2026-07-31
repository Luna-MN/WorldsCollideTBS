using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;
using Packets;

public class GameDataJSON
{
    public FactionDataJSON[] factions;
    public ArmyDataJSON[] armies;
    public UnitDataJSON[] units;
    public Dictionary<string, SkillDataJSON> skills = new Dictionary<string, SkillDataJSON>();
    public Dictionary<string, MovementDataJSON> movements = new Dictionary<string, MovementDataJSON>();
    
    [JsonConstructor]
    public GameDataJSON() { }
    public GameDataJSON(FactionDataJSON[] factions, ArmyDataJSON[] armies, UnitDataJSON[] units, Dictionary<string, SkillDataJSON> skills, Dictionary<string, MovementDataJSON> movements)
    {
        this.factions = factions;
        this.armies = armies;
        this.units = units;
        this.skills = skills;
        this.movements = movements;
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
    public UnitArmyDataJSON[] unitIds;
    [JsonConstructor]
    public ArmyDataJSON(){}
    public ArmyDataJSON(ArmyData army)
    {
        ID = army.Id;
        Name = army.Name;
        Description = army.Description;
        var uIDList = new List<UnitArmyDataJSON>();
        foreach (var unit in army.UnitIds)
        {
            uIDList.Add(new UnitArmyDataJSON(unit.UnitId, unit.Count));
        }
        unitIds = uIDList.ToArray();
        GD.Print(unitIds.Length + " units" + unitIds[0].unitId);
    }
}

public class UnitArmyDataJSON
{
    public long unitId;
    public int count;
    [JsonConstructor]
    public UnitArmyDataJSON(){}
    public UnitArmyDataJSON(long unitId, int count)
    {
        this.unitId = unitId;
        this.count = count;
    }
}

public class UnitDataJSON
{
    public long ID;
    public string Name;
    public string Skills;
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
        Skills = unit.Skills;
        Movement = unit.Movement;
        MaxHP = unit.MaxHP;
        AP = unit.AP;
        speed = (long)unit.Speed;
        factionId = unit.FactionId;
    }
}

public class SkillDataJSON
{
    public long ID;
    public string Name;
    public string Description;
    public SkillType Type;
    public long Cooldown;
    public long AP;
    public long Range;
    public string CombatString;
    public bool Universal;

    [JsonConstructor]
    public SkillDataJSON()
    {
        
    }

    public SkillDataJSON(SkillDataMessage Data)
    {
        ID = Data.Id;
        Name = Data.Name;
        Description = Data.Desc;
        Type = Data.Type;
        Cooldown = Data.Cooldown;
        AP = Data.AP;
        Range = Data.Range;
        CombatString = Data.CombatString;
        Universal = Data.Universal;
    }
}

public class MovementDataJSON
{
    public long ID;
    public string Name;
    public string Description;
    public long MovementCost;

    [JsonConstructor]
    public MovementDataJSON()
    {
        
    }

    public MovementDataJSON(MovementDataMessage Data)
    {
        ID = Data.Id;
        Name = Data.Name;
        Description = Data.Desc;
        MovementCost = Data.MoveCost;
    }
}