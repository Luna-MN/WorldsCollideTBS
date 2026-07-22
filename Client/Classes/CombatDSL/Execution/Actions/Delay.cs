using Packets.Classes.CombatDSL.Execution;

namespace DSLActions;

public class Delay(int Turns, Func Then) : UniversalCombatAction
{
    public void Execute(CombatContext context, int skillId)
    {
        throw new System.NotImplementedException();
    }

    public void Invert(int skillId)
    {
        throw new System.NotImplementedException();
    }
}