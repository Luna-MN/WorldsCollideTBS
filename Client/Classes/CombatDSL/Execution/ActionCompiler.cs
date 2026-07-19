using System;
using System.Collections.Generic;

public sealed class SkillCompiler
{
    public ICombatAction Compile(DSLExpr expr)
    {
        switch (expr)
        {
            case Func func:
                return CompileFunc(func);
            case EType effect:
                return CompileEType(effect);
            default:
                throw new NotSupportedException($"Unsupported DSL expression: {expr.GetType().Name}");
        }
        return null;
    }

    private ICombatAction CompileFunc(Func func)
    {
        switch (func)
        {
            case Inflict inflict:
                return CompileInflict(inflict);
                break;

            case Delay delay:
                return CompileDelay(delay);
                break;

            default:
                throw new NotSupportedException($"Unsupported function: {func.GetType().Name}");
        }
        return null;
    }

    private ICombatAction CompileInflict(Inflict inflict)
    {
        return new DSLActions.Inflict(Compile(inflict.EType), inflict.Turns);
    }

    private ICombatAction CompileDelay(Delay delay)
    {
        return new DSLActions.Delay(delay.Turns, delay.Then);
    }

    private static int ResolveTurns(CType ctype)
    {
        return ctype switch
        {
            Turns turns => turns.Amount,
            _ => throw new NotSupportedException($"Unsupported condition type: {ctype.GetType().Name}")
        };
    }

    private ICombatAction CompileEType(EType effect)
    {
        switch (effect)
        {
            case Damage damage:
                return new DSLActions.Damage(damage.Amount);
            case Heal heal:
                return new DSLActions.Heal(heal.Amount);
            case Poison poison:
                return new DSLActions.Poison(poison.Amount);
            case Bleed bleed:
                return new DSLActions.Bleed(bleed.Amount);
            case Comp comp:
                return new DSLActions.Comp(comp.Effects);
            default:
                throw new NotSupportedException($"Unsupported effect type: {effect.GetType().Name}");
        }
        return null;
    }
    

    private void ApplyDamage(int amount)
    {
        Console.WriteLine($"Damage: {amount}");
    }

    private void ApplyHeal(int amount)
    {
        Console.WriteLine($"Heal: {amount}");
    }

    private void ApplyPoison(int amount, int turns)
    {
        Console.WriteLine($"Poison: {amount} for {turns} turns");
    }

    private void ApplyBleed(int amount, int turns)
    {
        Console.WriteLine($"Bleed: {amount} for {turns} turns");
    }
}