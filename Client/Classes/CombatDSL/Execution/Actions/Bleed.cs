using System.Collections.Generic;
using Packets.Classes.CombatDSL.Execution;

namespace DSLActions;

public class Bleed(int Amount) : UniversalCombatAction
{
    public override void Execute(CombatContext context, int skillId)
    {
        base.Execute(context, skillId);
        
        SkillContext[skillId].Total += Amount;
        SkillContext[skillId].Context.Target.Damage(Amount);
    }

    public override void Invert(int skillId)
    {
        SkillContext[skillId].Context.Target.Heal(SkillContext[skillId].Total);
        SkillContext.Remove(skillId);
    }
}