using Packets.Classes.CombatDSL.Execution;

namespace DSLActions;

public class Heal(int amount) : UniversalCombatAction
{
    public override void Execute(CombatContext context, int skillId)
    {
        base.Execute(context, skillId);
        
        SkillContext[skillId].Total += amount;
        SkillContext[skillId].Context.Target.Heal(amount);
    }
    
    public override void Invert(int skillId)
    {
        SkillContext[skillId].Context.Target.Damage(SkillContext[skillId].Total);
        SkillContext.Remove(skillId);
    }
}