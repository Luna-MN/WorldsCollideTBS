using System.Collections.Generic;


// Kinds 
public enum DslValueKind
{
    EType,
    CType,
    Func
}

public interface DSLExpr;

public interface EType : DSLExpr;

public interface CType : DSLExpr;

public interface Func : DSLExpr;

// Types
public sealed record Damage(int Amount) : EType;

public sealed record Heal(int Amount) : EType;

public sealed record Poison(int Amount) : EType;

public sealed record Bleed(int Amount) : EType;

public sealed record Comp(IReadOnlyList<EType> Effects) : EType;

public sealed record Turns(int Amount) : CType;

public sealed record Inflict(EType EType, int Turns) : Func;

public sealed record Delay(int Turns, Func Then) : Func;