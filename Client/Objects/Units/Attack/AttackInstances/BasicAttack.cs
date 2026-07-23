
using Godot;
using Packets;
using Packets.Objects.Units.Attack;
using Packets.Util;

public class BasicAttack : UniversalAttack
{
    public override string Name()
    {
        return "BasicAttack";
    }
    
    public override void Init(IUnit unit, Node3D node, SkillData data)
    {
        base.Init(unit, node, data);
        CombatString($"D({Data.Damage})");
    }
}