namespace DSLActions;

public class Damage(int amount) : ICombatAction
{
    public int Amount { get; } = amount;

    public void Execute(CombatContext context)
    {
        context.Target.Damage(Amount);
    }

}