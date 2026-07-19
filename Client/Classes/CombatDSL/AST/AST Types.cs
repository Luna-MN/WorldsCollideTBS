using System.Collections.Generic;


public readonly record struct TextSpan(int Start, int Length)
{
    public int End => Start + Length;
}

public abstract record Expr
{
    public TextSpan Span { get; }

    protected Expr(TextSpan span)
    {
        Span = span;
    }
}

public sealed record IntLit : Expr
{
    public int Value { get; }

    public IntLit(int value, TextSpan span) : base(span)
    {
        Value = value;
    }
}

public sealed record Call : Expr
{
    public string Name { get; }
    public IReadOnlyList<Expr> Args { get; }

    public Call(string name, IReadOnlyList<Expr> args, TextSpan span) : base(span)
    {
        Name = name;
        Args = args;
    }
}

