#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

public sealed record DslCompileOptions(
    bool AllowZeroDelay = true,
    bool AllowZeroDuration = false,
    bool AllowZeroEffectAmount = false
);

public sealed class DslCompileException(string message, int position) : Exception($"Compile error at character {position + 1}: {message}")
{
    public int Position { get; } = position;
}

public sealed class DslCompiler
{
    private sealed record Entry<T>(
        string Signature,
        int MinArgs,
        int? MaxArgs,
        Func<Call, DslCompiler, T> Build
    );

    private readonly Dictionary<string, Entry<EType>> _etype = new();
    private readonly Dictionary<string, Entry<CType>> _ctype = new();
    private readonly Dictionary<string, Entry<Func>> _func = new();

    public DslCompileOptions Options { get; }

    public DslCompiler(DslCompileOptions? options = null)
    {
        Options = options ?? new DslCompileOptions();
        Register();
    }

    public DSLExpr Compile(string text)
    {
        Expr expr = Parser.Parse(text);
        return Compile(expr);
    }
    public Func CompileFunc(string text)
    {
        Expr expr = Parser.Parse(text);
        return CompileFunc(expr);
    }

    public EType CompileEType(string text)
    {
        Expr expr = Parser.Parse(text);
        return CompileEType(expr);
    }

    public CType CompileCType(string text)
    {
        Expr expr = Parser.Parse(text);
        return CompileCType(expr);
    }

    public DSLExpr Compile(Expr expr)
    {
        if (expr is Call call)
        {
            if (_func.TryGetValue(call.Name, out Entry<Func>? actionEntry))
            {
                ValidateArgCount(call, actionEntry.Signature, actionEntry.MinArgs, actionEntry.MaxArgs);
                return actionEntry.Build(call, this);
            }

            if (_etype.TryGetValue(call.Name, out Entry<EType>? etypeEntry))
            {
                ValidateArgCount(call, etypeEntry.Signature, etypeEntry.MinArgs, etypeEntry.MaxArgs);
                return etypeEntry.Build(call, this);
            }

            if (_ctype.TryGetValue(call.Name, out Entry<CType>? ctypeEntry))
            {
                ValidateArgCount(call, ctypeEntry.Signature, ctypeEntry.MinArgs, ctypeEntry.MaxArgs);
                return ctypeEntry.Build(call, this);
            }

            throw Error(call, $"Unknown Call '{call.Name}'.");
        }

        throw Error(expr, $"Unknown Expression '{expr}'.");
    }

    public Func CompileFunc(Expr expr)
    {
        if (expr is not Call call)
            throw Error(expr, $"Expected Func but got {Describe(expr)}.");

        if (_func.TryGetValue(call.Name, out Entry<Func>? entry))
        {
            ValidateArgCount(call, entry.Signature, entry.MinArgs, entry.MaxArgs);
            return entry.Build(call, this);
        }

        DslValueKind? actualKind = FindKind(call.Name);

        if (actualKind is not null)
            throw Error(call, $"'{call.Name}' returns {KindName(actualKind.Value)}, expected Func.");

        throw Error(call, $"Unknown Func '{call.Name}'.");
    }

    public EType CompileEType(Expr expr)
    {
        if (expr is not Call call)
            throw Error(expr, $"Expected EType but got {Describe(expr)}.");

        if (_etype.TryGetValue(call.Name, out Entry<EType>? entry))
        {
            ValidateArgCount(call, entry.Signature, entry.MinArgs, entry.MaxArgs);
            return entry.Build(call, this);
        }

        DslValueKind? actualKind = FindKind(call.Name);

        if (actualKind is not null)
            throw Error(call, $"'{call.Name}' returns {KindName(actualKind.Value)}, expected EType.");

        throw Error(call, $"Unknown EType '{call.Name}'.");
    }

    public CType CompileCType(Expr expr)
    {
        if (expr is not Call call)
            throw Error(expr, $"Expected CType but got {Describe(expr)}.");

        if (_ctype.TryGetValue(call.Name, out Entry<CType>? entry))
        {
            ValidateArgCount(call, entry.Signature, entry.MinArgs, entry.MaxArgs);
            return entry.Build(call, this);
        }

        DslValueKind? actualKind = FindKind(call.Name);

        if (actualKind is not null)
            throw Error(call, $"'{call.Name}' returns {KindName(actualKind.Value)}, expected CType.");

        throw Error(call, $"Unknown CType '{call.Name}'.");
    }

    public void RegisterEType(
        string name,
        int minArgs,
        int? maxArgs,
        string signature,
        Func<Call, DslCompiler, EType> build)
    {
        ValidateName(name);
        _etype[name] = new Entry<EType>(signature, minArgs, maxArgs, build);
    }
    

    public void RegisterFunc(
        string name,
        int minArgs,
        int? maxArgs,
        string signature,
        Func<Call, DslCompiler, Func> build)
    {
        ValidateName(name);
        _func[name] = new Entry<Func>(signature, minArgs, maxArgs, build);
    }

    public int IntArg(Call call, int index, string signature)
    {
        Expr arg = call.Args[index];

        if (arg is IntLit intLit)
            return intLit.Value;

        throw Error(
            arg,
            $"{call.Name} expects {signature}, but arg{index + 1} was {Describe(arg)}."
        );
    }

    public int PositiveIntArg(
        Call call,
        int index,
        string signature,
        string parameterName,
        bool allowZero)
    {
        int value = IntArg(call, index, signature);

        if (value < 0)
        {
            throw Error(
                call.Args[index],
                $"{call.Name} expects {signature}, but {parameterName} must not be negative."
            );
        }

        if (!allowZero && value == 0)
        {
            throw Error(
                call.Args[index],
                $"{call.Name} expects {signature}, but {parameterName} must be greater than 0."
            );
        }

        return value;
    }

    public EType ETypeArg(Call call, int index, string signature)
    {
        try
        {
            return CompileEType(call.Args[index]);
        }
        catch (DslCompileException ex)
        {
            throw Error(
                call.Args[index],
                $"{call.Name} expects {signature}, but arg{index + 1} was invalid. {ex.Message}"
            );
        }
    }

    public CType CTypeArg(Call call, int index, string signature)
    {
        try
        {
            return CompileCType(call.Args[index]);
        }
        catch (DslCompileException ex)
        {
            throw Error(
                call.Args[index],
                $"{call.Name} expects {signature}, but arg{index + 1} was invalid. {ex.Message}"
            );
        }
    }

    public Func FuncArg(Call call, int index, string signature)
    {
        try
        {
            return CompileFunc(call.Args[index]);
        }
        catch (DslCompileException ex)
        {
            throw Error(
                call.Args[index],
                $"{call.Name} expects {signature}, but arg{index + 1} was invalid. {ex.Message}"
            );
        }
    }

    private void Register()
    {
        RegisterEType(
            "D",
            1,
            1,
            "D(amount:int) -> EType",
            static (call, compiler) =>
            {
                int amount = compiler.PositiveIntArg(
                    call,
                    0,
                    "D(amount:int) -> EType",
                    "amount",
                    compiler.Options.AllowZeroEffectAmount
                );

                return new Damage(amount);
            }
        );

        RegisterEType(
            "H",
            1,
            1,
            "H(amount:int) -> EType",
            static (call, compiler) =>
            {
                int amount = compiler.PositiveIntArg(
                    call,
                    0,
                    "H(amount:int) -> EType",
                    "amount",
                    compiler.Options.AllowZeroEffectAmount
                );

                return new Heal(amount);
            }
        );

        RegisterEType(
            "P",
            1,
            1,
            "P(amount:int) -> EType",
            static (call, compiler) =>
            {
                int amount = compiler.PositiveIntArg(
                    call,
                    0,
                    "P(amount:int) -> EType",
                    "amount",
                    compiler.Options.AllowZeroEffectAmount
                );

                return new Poison(amount);
            }
        );

        RegisterEType(
            "B",
            1,
            1,
            "B(amount:int) -> EType",
            static (call, compiler) =>
            {
                int amount = compiler.PositiveIntArg(
                    call,
                    0,
                    "B(amount:int) -> EType",
                    "amount",
                    compiler.Options.AllowZeroEffectAmount
                );

                return new Bleed(amount);
            }
        );

        RegisterEType(
            "C",
            1,
            null,
            "C(effects:EType...) -> EType",
            static (call, compiler) =>
            {
                EType[] effects = call.Args
                    .Select(compiler.CompileEType)
                    .ToArray();

                return new Comp(effects);
            }
        );

        RegisterFunc(
            "I",
            2,
            2,
            "I(effect:EType, turns:int) -> Func",
            static (call, compiler) =>
            {
                const string signature = "I(effect:EType, turns:int) -> Func";

                EType eType = compiler.ETypeArg(call, 0, signature);
                int turns = compiler.PositiveIntArg(
                    call,
                    1,
                    signature,
                    "turns",
                    compiler.Options.AllowZeroDelay
                );

                return new Inflict(eType, turns);
            }
        );

        RegisterFunc(
            "De",
            2,
            2,
            "De(turns:int, then:Func) -> Func",
            static (call, compiler) =>
            {
                const string signature = "De(turns:int, then:Func) -> Func";

                int turns = compiler.PositiveIntArg(
                    call,
                    0,
                    signature,
                    "turns",
                    compiler.Options.AllowZeroDelay
                );

                Func then = compiler.FuncArg(call, 1, signature);

                return new Delay(turns, then);
            }
        );
    }

    private void ValidateArgCount(Call call, string signature, int minArgs, int? maxArgs)
    {
        int count = call.Args.Count;

        bool tooFew = count < minArgs;
        bool tooMany = maxArgs is not null && count > maxArgs.Value;

        if (!tooFew && !tooMany)
            return;

        string expected = maxArgs is null || minArgs != maxArgs.Value
            ? $"{minArgs}+ args"
            : $"{minArgs} args";

        throw Error(
            call,
            $"{call.Name} expects {signature}, but got {count} args. Expected {expected}."
        );
    }

    private DslValueKind? FindKind(string name)
    {
        if (_etype.ContainsKey(name))
            return DslValueKind.EType;

        if (_ctype.ContainsKey(name))
            return DslValueKind.CType;

        if (_func.ContainsKey(name))
            return DslValueKind.Func;

        return null;
    }

    private static string KindName(DslValueKind kind)
    {
        return kind switch
        {
            DslValueKind.EType => "EType",
            DslValueKind.CType => "CType",
            DslValueKind.Func => "Func",
            _ => kind.ToString()
        };
    }

    private static string Describe(Expr expr)
    {
        return expr switch
        {
            IntLit => "int",
            Call call => $"call '{call.Name}'",
            _ => expr.GetType().Name
        };
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("DSL function name cannot be empty.", nameof(name));

        if (!char.IsLetter(name[0]))
            throw new ArgumentException("DSL function name must start with a letter.", nameof(name));

        foreach (char c in name)
        {
            if (!char.IsLetterOrDigit(c))
                throw new ArgumentException("DSL function name may only contain letters and digits.", nameof(name));
        }
    }

    private static DslCompileException Error(Expr expr, string message)
    {
        return new DslCompileException(message, expr.Span.Start);
    }
}