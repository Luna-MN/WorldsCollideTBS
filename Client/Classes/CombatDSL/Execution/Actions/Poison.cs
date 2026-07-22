using Packets.Classes.CombatDSL.Execution;

namespace DSLActions;

public class Poison(int amount) : UniversalCombatAction
{
    public override void Execute(CombatContext context, int skillId)
    {
        base.Execute(context, skillId);
        SkillContext[skillId].Total += amount;
        throw new System.NotImplementedException();
    }
    public override void Invert(int skillId)
    {
        SkillContext[skillId].Context.Target.Heal(SkillContext[skillId].Total);
        SkillContext.Remove(skillId);
    }
}