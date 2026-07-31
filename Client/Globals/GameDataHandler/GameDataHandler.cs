using System.Collections.Generic;
using Godot;

public class GameDataHandler
{
    private GameDataJSON gameData;
    private Dictionary<long, UnitDataJSON> units = new();
    private Dictionary<long, Dictionary<long, UnitDataJSON>> unitsByArmy = new(); // armyID, unitID, unit
    private Dictionary<long, FactionDataJSON> factions = new();
    private Dictionary<long, Dictionary<long, UnitDataJSON>> unitsByFaction = new(); // factionID, unitID, unit
    private Dictionary<long, ArmyDataJSON> armies = new();
    private Dictionary<long, Dictionary<long, ArmyDataJSON>> armiesByFaction = new(); // factionID, armyID, army
    

    public void Init(GameDataJSON GD)
    {
        Godot.GD.Print(GD.factions);
        gameData = GD;
        createDictionaries();
    }

    private void createDictionaries()
    {
        foreach (var faction in gameData.factions)
        {
            factions[faction.ID] = faction;
        }
        foreach (var unit in gameData.units)
        {
            units[unit.ID] = unit;
        }
        foreach (var army in gameData.armies)
        {
            armies[army.ID] = army;
        }
        // unitsByFaction and armyByFaction
        foreach (var f in factions)
        {
            unitsByFaction[f.Key] = PopulateUnitsForFaction(f.Key);    
            armiesByFaction[f.Key] = PopulateArmiesForFaction(f.Key);    
        }
        // unitsByArmy
        foreach (var a in armies)
        {
            unitsByArmy[a.Key] = PopulateUnitsForArmy(a.Key);    
        }
    }
    
    public List<FactionDataJSON> GetFactions() => new(factions.Values);
    public UnitDataJSON GetUnit(long id) => units[id];
    public FactionDataJSON GetFaction(long id) => factions[id];
    public ArmyDataJSON GetArmy(long id) => armies[id];
    public SkillDataJSON GetSkill(string name) => gameData.skills[name];
    public MovementDataJSON GetMovement(string name) => gameData.movements[name];
    public Dictionary<long, UnitDataJSON> GetUnitsForFaction(long factionId) => unitsByFaction[factionId];
    public Dictionary<long, ArmyDataJSON> GetArmiesForFaction(long factionId) => armiesByFaction[factionId];
    public Dictionary<long, UnitDataJSON> GetUnitsForArmy(long armyId) => unitsByArmy[armyId];

    #region Dictionay Setters
        private Dictionary<long, UnitDataJSON> PopulateUnitsForArmy(long armyId)
        {
            var unitDict = new Dictionary<long, UnitDataJSON>();

            var army = GetArmy(armyId);
            if (army == null)
            {
                GD.PushError($"PopulateUnitsForArmy: Could not find army with ID {armyId}.");
                return unitDict;
            }

            if (army.unitIds == null)
            {
                GD.PushError($"PopulateUnitsForArmy: Army {armyId} has null unitIds.");
                return unitDict;
            }

            foreach (var uid in army.unitIds)
            {
                if (uid == null)
                {
                    GD.PushError($"PopulateUnitsForArmy: Army {armyId} contains a null unit ID entry.");
                    continue;
                }

                var unit = GetUnit(uid.unitId);
                if (unit == null)
                {
                    GD.PushError($"PopulateUnitsForArmy: Could not find unit with ID {uid.unitId} for army {armyId}.");
                    continue;
                }

                unitDict[uid.unitId] = unit;
            }

            return unitDict;
        }
        private Dictionary<long, ArmyDataJSON> PopulateArmiesForFaction(long factionId)
        {
            var armyDict = new Dictionary<long, ArmyDataJSON>();
            var faction = GetFaction(factionId);
            foreach (var armyId in faction.armyIds)
            {
                armyDict[armyId] = GetArmy(armyId);
            }
            return armyDict;
        }
        private Dictionary<long, UnitDataJSON> PopulateUnitsForFaction(long factionId)
        {
            var unitDict = new Dictionary<long, UnitDataJSON>();
            foreach (var u in units.Values)
            {
                if (u.factionId == factionId)
                {
                    unitDict[u.ID] = u;    
                }
            }
            return unitDict;
        }
    #endregion

}