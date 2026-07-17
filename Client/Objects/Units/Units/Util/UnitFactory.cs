using System;
using System.Linq;
using System.Reflection;
using Google.Protobuf.Collections;

public static class UnitFactory
{
    public static UnitData UnwrapUnitData(Packets.UnitMessage unit)
    {
        var UD = new UnitData()
        {
            UnitName = unit.UnitName,
            MaxAP = unit.AP,
            HP = unit.HP,
            MaxHP = unit.MaxHP,
            OwnerId = unit.OwnerId,
            UnitId = unit.UnitId,
            Speed = (UnitData.Speeds)unit.Speed
        };
        return UD;
    }

    public static Packets.UnitMessage WrapUnitData(UnitData unit)
    {
        var unitPacket = new Packets.UnitMessage()
        {
            UnitName = unit.UnitName,
            AP = unit.MaxAP,
            HP = unit.HP,
            MaxHP = unit.MaxHP,
            OwnerId = unit.OwnerId,
            UnitId = unit.UnitId,
            Speed = (Packets.Speeds)unit.Speed
        };
        
        return unitPacket;
    }
    public static IUnit GetUnit(string UnitName)
    {
        // Get all types that implement IFeature
        var Units = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => typeof(IUnit).IsAssignableFrom(type) &&
                           !type.IsInterface &&
                           !type.IsAbstract &&
                           type.Name == UnitName).ToList();
        
        if (Units.Count == 0)
        {
            throw new InvalidOperationException("No implementations of IUnit found. With name: " + UnitName);
        }
        
        // Select the first one (should be impossible for there to be more than one thing in this list)
        var unit = Units[0];
        // Create an instance of the random type
        return (IUnit)Activator.CreateInstance(unit);
    }
}