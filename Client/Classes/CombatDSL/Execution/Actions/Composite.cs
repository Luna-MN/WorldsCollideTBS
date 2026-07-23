using System.Collections.Generic;
using Packets.Classes.CombatDSL.Execution;

namespace DSLActions;

public class Composite(IReadOnlyList<ICombatAction> list) : UniversalCombatAction
{
    public override void Execute(CombatContext context, int skillId)
    {
        foreach (var e in list)
        {
            e.Execute(context, skillId);
        }
    }

    public override void Invert(int skillId)
    {
        foreach (var e in list)
        {
            e.Invert(skillId);
        }
    }
}