using Godot;

namespace Packets.Objects.Units.Attack;

public class UniversalAttack : UniversalSkill
{
    public AttackData Data { get; set; }
    public override string Name()
    {
        return "UniversalAttack";
    }
    public override SkillType Type()
    {
        return SkillType.Attack;
    }
    
    public override void Init(IUnit unit, Node3D node, SkillData data)
    {
        base.Init(unit, node, data);
        Data = data as AttackData;
    }

}