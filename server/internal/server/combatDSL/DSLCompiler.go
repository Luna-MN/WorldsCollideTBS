package combatDSL

import (
	"fmt"
	"server/internal/server/combatDSL/coreAST/tokens"
	"unicode"
)

type CompileOptions struct {
	AllowZeroDelay        bool
	AllowZeroDuration     bool
	AllowZeroEffectAmount bool
}

func DefaultCompileOptions() CompileOptions {
	return CompileOptions{
		AllowZeroDelay:        true,
		AllowZeroDuration:     false,
		AllowZeroEffectAmount: false,
	}
}

type entry[T any] struct {
	signature string
	minArgs   int
	maxArgs   *int
	build     func(*combatDSL.Call, *Compiler) (T, error)
}

type Compiler struct {
	options CompileOptions
	etype   map[string]entry[EType]
	ctype   map[string]entry[CType]
	fn      map[string]entry[FuncExpr]
}

func NewCompiler(options *CompileOptions) *Compiler {
	opts := DefaultCompileOptions()
	if options != nil {
		opts = *options
	}

	c := &Compiler{
		options: opts,
		etype:   make(map[string]entry[EType]),
		ctype:   make(map[string]entry[CType]),
		fn:      make(map[string]entry[FuncExpr]),
	}

	c.register()

	return c
}

func (c *Compiler) CompileText(text string) (DSLExpr, error) {
	expr, err := Parse(text)
	if err != nil {
		return nil, err
	}

	return c.Compile(expr)
}

func (c *Compiler) Compile(expr combatDSL.Expr) (DSLExpr, error) {
	call, ok := expr.(*combatDSL.Call)
	if !ok {
		return nil, c.error(expr, fmt.Sprintf("unknown expression '%v'", expr))
	}

	if entry, ok := c.fn[call.Name]; ok {
		if err := c.validateArgCount(call, entry.signature, entry.minArgs, entry.maxArgs); err != nil {
			return nil, err
		}

		return entry.build(call, c)
	}

	if entry, ok := c.etype[call.Name]; ok {
		if err := c.validateArgCount(call, entry.signature, entry.minArgs, entry.maxArgs); err != nil {
			return nil, err
		}

		return entry.build(call, c)
	}

	if entry, ok := c.ctype[call.Name]; ok {
		if err := c.validateArgCount(call, entry.signature, entry.minArgs, entry.maxArgs); err != nil {
			return nil, err
		}

		return entry.build(call, c)
	}

	return nil, c.error(call, "unknown call '"+call.Name+"'")
}

func (c *Compiler) CompileFunc(expr combatDSL.Expr) (FuncExpr, error) {
	call, ok := expr.(*combatDSL.Call)
	if !ok {
		return nil, c.error(expr, "expected Func but got "+describe(expr))
	}

	entry, ok := c.fn[call.Name]
	if !ok {
		if kind, found := c.findKind(call.Name); found {
			return nil, c.error(call, "'"+call.Name+"' returns "+kindName(kind)+", expected Func")
		}

		return nil, c.error(call, "unknown Func '"+call.Name+"'")
	}

	if err := c.validateArgCount(call, entry.signature, entry.minArgs, entry.maxArgs); err != nil {
		return nil, err
	}

	return entry.build(call, c)
}

func (c *Compiler) CompileEType(expr combatDSL.Expr) (EType, error) {
	call, ok := expr.(*combatDSL.Call)
	if !ok {
		return nil, c.error(expr, "expected EType but got "+describe(expr))
	}

	entry, ok := c.etype[call.Name]
	if !ok {
		if kind, found := c.findKind(call.Name); found {
			return nil, c.error(call, "'"+call.Name+"' returns "+kindName(kind)+", expected EType")
		}

		return nil, c.error(call, "unknown EType '"+call.Name+"'")
	}

	if err := c.validateArgCount(call, entry.signature, entry.minArgs, entry.maxArgs); err != nil {
		return nil, err
	}

	return entry.build(call, c)
}

func (c *Compiler) RegisterEType(name string, minArgs int, maxArgs *int, signature string, build func(*combatDSL.Call, *Compiler) (EType, error)) error {
	if err := validateName(name); err != nil {
		return err
	}

	c.etype[name] = entry[EType]{
		signature: signature,
		minArgs:   minArgs,
		maxArgs:   maxArgs,
		build:     build,
	}

	return nil
}

func (c *Compiler) RegisterFunc(name string, minArgs int, maxArgs *int, signature string, build func(*combatDSL.Call, *Compiler) (FuncExpr, error)) error {
	if err := validateName(name); err != nil {
		return err
	}

	c.fn[name] = entry[FuncExpr]{
		signature: signature,
		minArgs:   minArgs,
		maxArgs:   maxArgs,
		build:     build,
	}

	return nil
}

func (c *Compiler) intArg(call *combatDSL.Call, index int, signature string) (int, error) {
	arg := call.Args[index]

	intLit, ok := arg.(*combatDSL.IntLit)
	if ok {
		return intLit.Value, nil
	}

	return 0, c.error(
		arg,
		fmt.Sprintf("%s expects %s, but arg%d was %s", call.Name, signature, index+1, describe(arg)),
	)
}

func (c *Compiler) positiveIntArg(call *combatDSL.Call, index int, signature string, parameterName string, allowZero bool) (int, error) {
	value, err := c.intArg(call, index, signature)
	if err != nil {
		return 0, err
	}

	if value < 0 {
		return 0, c.error(
			call.Args[index],
			fmt.Sprintf("%s expects %s, but %s must not be negative", call.Name, signature, parameterName),
		)
	}

	if !allowZero && value == 0 {
		return 0, c.error(
			call.Args[index],
			fmt.Sprintf("%s expects %s, but %s must be greater than 0", call.Name, signature, parameterName),
		)
	}

	return value, nil
}

func (c *Compiler) etypeArg(call *combatDSL.Call, index int, signature string) (EType, error) {
	effect, err := c.CompileEType(call.Args[index])
	if err != nil {
		return nil, c.error(
			call.Args[index],
			fmt.Sprintf("%s expects %s, but arg%d was invalid. %s", call.Name, signature, index+1, err.Error()),
		)
	}

	return effect, nil
}

func (c *Compiler) funcArg(call *combatDSL.Call, index int, signature string) (FuncExpr, error) {
	fn, err := c.CompileFunc(call.Args[index])
	if err != nil {
		return nil, c.error(
			call.Args[index],
			fmt.Sprintf("%s expects %s, but arg%d was invalid. %s", call.Name, signature, index+1, err.Error()),
		)
	}

	return fn, nil
}

func (c *Compiler) register() {
	one := 1
	two := 2

	_ = c.RegisterEType("D", 1, &one, "D(amount:int) -> EType", func(call *combatDSL.Call, compiler *Compiler) (EType, error) {
		amount, err := compiler.positiveIntArg(call, 0, "D(amount:int) -> EType", "amount", compiler.options.AllowZeroEffectAmount)
		if err != nil {
			return nil, err
		}

		return &DamageExpr{Amount: amount}, nil
	})

	_ = c.RegisterEType("H", 1, &one, "H(amount:int) -> EType", func(call *combatDSL.Call, compiler *Compiler) (EType, error) {
		amount, err := compiler.positiveIntArg(call, 0, "H(amount:int) -> EType", "amount", compiler.options.AllowZeroEffectAmount)
		if err != nil {
			return nil, err
		}

		return &HealExpr{Amount: amount}, nil
	})

	_ = c.RegisterEType("P", 1, &one, "P(amount:int) -> EType", func(call *combatDSL.Call, compiler *Compiler) (EType, error) {
		amount, err := compiler.positiveIntArg(call, 0, "P(amount:int) -> EType", "amount", compiler.options.AllowZeroEffectAmount)
		if err != nil {
			return nil, err
		}

		return &PoisonExpr{Amount: amount}, nil
	})

	_ = c.RegisterEType("B", 1, &one, "B(amount:int) -> EType", func(call *combatDSL.Call, compiler *Compiler) (EType, error) {
		amount, err := compiler.positiveIntArg(call, 0, "B(amount:int) -> EType", "amount", compiler.options.AllowZeroEffectAmount)
		if err != nil {
			return nil, err
		}

		return &BleedExpr{Amount: amount}, nil
	})

	_ = c.RegisterEType("C", 1, nil, "C(effects:EType...) -> EType", func(call *combatDSL.Call, compiler *Compiler) (EType, error) {
		effects := make([]EType, 0, len(call.Args))

		for _, arg := range call.Args {
			effect, err := compiler.CompileEType(arg)
			if err != nil {
				return nil, err
			}

			effects = append(effects, effect)
		}

		return &CompositeExpr{Effects: effects}, nil
	})

	_ = c.RegisterFunc("I", 2, &two, "I(effect:EType, turns:int) -> Func", func(call *combatDSL.Call, compiler *Compiler) (FuncExpr, error) {
		const signature = "I(effect:EType, turns:int) -> Func"

		effect, err := compiler.etypeArg(call, 0, signature)
		if err != nil {
			return nil, err
		}

		turns, err := compiler.positiveIntArg(call, 1, signature, "turns", compiler.options.AllowZeroDelay)
		if err != nil {
			return nil, err
		}

		return &InflictExpr{
			Effect: effect,
			Turns:  turns,
		}, nil
	})

	_ = c.RegisterFunc("De", 2, &two, "De(turns:int, then:Func) -> Func", func(call *combatDSL.Call, compiler *Compiler) (FuncExpr, error) {
		const signature = "De(turns:int, then:Func) -> Func"

		turns, err := compiler.positiveIntArg(call, 0, signature, "turns", compiler.options.AllowZeroDelay)
		if err != nil {
			return nil, err
		}

		then, err := compiler.funcArg(call, 1, signature)
		if err != nil {
			return nil, err
		}

		return &DelayExpr{
			Turns: turns,
			Then:  then,
		}, nil
	})
}

func (c *Compiler) validateArgCount(call *combatDSL.Call, signature string, minArgs int, maxArgs *int) error {
	count := len(call.Args)

	tooFew := count < minArgs
	tooMany := maxArgs != nil && count > *maxArgs

	if !tooFew && !tooMany {
		return nil
	}

	expected := fmt.Sprintf("%d+ args", minArgs)
	if maxArgs != nil && minArgs == *maxArgs {
		expected = fmt.Sprintf("%d args", minArgs)
	}

	return c.error(
		call,
		fmt.Sprintf("%s expects %s, but got %d args. Expected %s", call.Name, signature, count, expected),
	)
}

func (c *Compiler) findKind(name string) (ValueKind, bool) {
	if _, ok := c.etype[name]; ok {
		return ValueKindEType, true
	}

	if _, ok := c.ctype[name]; ok {
		return ValueKindCType, true
	}

	if _, ok := c.fn[name]; ok {
		return ValueKindFunc, true
	}

	return 0, false
}

func (c *Compiler) error(expr combatDSL.Expr, message string) error {
	return &combatDSL.CompileError{
		Message:  message,
		Position: expr.Span().Start,
	}
}

func kindName(kind ValueKind) string {
	switch kind {
	case ValueKindEType:
		return "EType"
	case ValueKindCType:
		return "CType"
	case ValueKindFunc:
		return "Func"
	default:
		return "unknown"
	}
}

func describe(expr combatDSL.Expr) string {
	switch e := expr.(type) {
	case *combatDSL.IntLit:
		return "int"
	case *combatDSL.Call:
		return "call '" + e.Name + "'"
	default:
		return fmt.Sprintf("%T", expr)
	}
}

func validateName(name string) error {
	if name == "" {
		return fmt.Errorf("DSL function name cannot be empty")
	}

	runes := []rune(name)

	if !unicode.IsLetter(runes[0]) {
		return fmt.Errorf("DSL function name must start with a letter")
	}

	for _, r := range runes {
		if !unicode.IsLetter(r) && !unicode.IsDigit(r) {
			return fmt.Errorf("DSL function name may only contain letters and digits")
		}
	}

	return nil
}
