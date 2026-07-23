
using System.Collections.Generic;

public sealed class CombatContext(IUnit caster, IUnit target)
{
    public IUnit Caster { get; } = caster;
    public IUnit Target { get; } = target;
}

public sealed class SkillContext(CombatContext context, int skillId)
{
    public CombatContext Context { get; } = context;
    public int SkillId { get; } = skillId;
    public int Total { get; set; }
}

public interface ICombatAction
{
    Dictionary<int, SkillContext> SkillContext { get; } // <SkillId, SkillContext>
    
    void Execute(CombatContext context, int skillId);
    void Invert(int skillId);
}