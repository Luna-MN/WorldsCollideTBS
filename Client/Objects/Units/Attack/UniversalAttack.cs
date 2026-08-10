using Godot;

namespace Packets.Objects.Units.Attack;

public class UniversalAttack : UniversalSkill
{
    public override string Name()
    {
        return Data.Name;
    }
    
    public override void Init(IUnit unit, Node3D node, SkillDataJSON data)
    {
        base.Init(unit, node, data);
        Data = data;
    }

}