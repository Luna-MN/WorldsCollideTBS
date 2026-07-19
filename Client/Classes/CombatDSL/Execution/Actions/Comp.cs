using System.Collections.Generic;

namespace DSLActions;

public class Comp(IReadOnlyList<EType> list) : ICombatAction
{
    public void Execute(CombatContext context)
    {
        throw new System.NotImplementedException();
    }
}