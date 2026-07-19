

using System;
using System.Collections.Generic;

public sealed class DslParseException(string message, int position) : Exception($"Parse error at character {position + 1}: {message}")
{
    public int Position { get; } = position;
}

public enum TokenKind
{
    Identifier,
    Integer,
    LeftParen,
    RightParen,
    Comma,
    End
}

public readonly record struct Token(
    TokenKind Kind,
    string Text,
    int IntValue,
    int Position
);

public enum SkillETypeCode
{
    Damage,
    Heal,
    Poison,
    Bleed
}

public readonly record struct SkillInstruction(
    SkillETypeCode ETypeCode,
    int IntArgument
);

public sealed class CompiledSkill
{
    public IReadOnlyList<SkillInstruction> Instructions { get; }

    public CompiledSkill(IReadOnlyList<SkillInstruction> instructions)
    {
        Instructions = instructions;
    }
}

public sealed class DslCall
{
    public string Name { get; }
    public IReadOnlyList<DslArgument> Arguments { get; }

    public DslCall(string name, IReadOnlyList<DslArgument> arguments)
    {
        Name = name;
        Arguments = arguments;
    }
}

public readonly record struct DslArgument(int IntValue);
