using System.Collections.Generic;

namespace Packets.Classes.CombatDSL.Execution;

public class UniversalCombatAction : ICombatAction
{
    public Dictionary<int, SkillContext> SkillContext { get; } = new Dictionary<int, SkillContext>();
    public virtual void Execute(CombatContext context, int skillId)
    {
        if (!SkillContext.ContainsKey(skillId))
        {
            SkillContext.Add(skillId, new SkillContext(context, skillId));
            context.Caster.AddToSkillBuffer(skillId, this);
        }
    }

    public virtual void Invert(int skillId)
    {
    }
}