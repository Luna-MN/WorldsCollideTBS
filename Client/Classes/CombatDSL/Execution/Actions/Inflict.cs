using Packets.Classes.CombatDSL.Execution;

namespace DSLActions;

public class Inflict(ICombatAction eType, int turns) : UniversalCombatAction
{
    public override void Execute(CombatContext context, int skillId)
    {
        base.Execute(context, skillId);
        context.Target.Inflict(skillId, eType, turns);
    }
    public override void Invert(int skillId)
    {
        SkillContext[skillId].Context.Caster.RemoveInflict(skillId);
    }
    
}