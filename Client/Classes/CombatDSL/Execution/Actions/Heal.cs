namespace DSLActions;

public class Heal(int amount) : ICombatAction
{
    public void Execute(CombatContext context)
    {
        context.Target.Heal(amount);
    }
}