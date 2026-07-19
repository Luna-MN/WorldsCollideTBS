
public sealed class CombatContext
{
    public IUnit Caster { get; }
    public IUnit Target { get; }

    public CombatContext(IUnit caster, IUnit target)
    {
        Caster = caster;
        Target = target;
    }
}

public interface ICombatAction
{
    void Execute(CombatContext context);
}