package combatDSL

import "fmt"

type SkillCompiler struct {
	dslCompiler *Compiler
}

func NewSkillCompiler(dslCompiler *Compiler) *SkillCompiler {
	if dslCompiler == nil {
		dslCompiler = NewCompiler(nil)
	}

	return &SkillCompiler{
		dslCompiler: dslCompiler,
	}
}

func (c *SkillCompiler) CompileText(text string) (CombatAction, error) {
	expr, err := c.dslCompiler.CompileText(text)
	if err != nil {
		return nil, err
	}

	return c.Compile(expr)
}

func (c *SkillCompiler) Compile(expr DSLExpr) (CombatAction, error) {
	switch e := expr.(type) {
	case FuncExpr:
		return c.compileFunc(e)
	case EType:
		return c.compileEType(e)
	default:
		return nil, fmt.Errorf("unsupported DSL expression: %T", expr)
	}
}

func (c *SkillCompiler) compileFunc(fn FuncExpr) (CombatAction, error) {
	switch f := fn.(type) {
	case *InflictExpr:
		return c.compileInflict(f)
	case *DelayExpr:
		return c.compileDelay(f)
	default:
		return nil, fmt.Errorf("unsupported function: %T", fn)
	}
}

func (c *SkillCompiler) compileInflict(inflict *InflictExpr) (CombatAction, error) {
	effect, err := c.Compile(inflict.Effect)
	if err != nil {
		return nil, err
	}

	return NewInflictAction(effect, inflict.Turns), nil
}

func (c *SkillCompiler) compileDelay(delay *DelayExpr) (CombatAction, error) {
	return NewDelayAction(delay.Turns, delay.Then), nil
}

func (c *SkillCompiler) compileEType(effect EType) (CombatAction, error) {
	switch e := effect.(type) {
	case *DamageExpr:
		return NewDamageAction(e.Amount), nil
	case *HealExpr:
		return NewHealAction(e.Amount), nil
	case *PoisonExpr:
		return NewPoisonAction(e.Amount), nil
	case *BleedExpr:
		return NewBleedAction(e.Amount), nil
	case *CompositeExpr:
		actions := make([]CombatAction, 0, len(e.Effects))

		for _, effect := range e.Effects {
			action, err := c.Compile(effect)
			if err != nil {
				return nil, err
			}

			actions = append(actions, action)
		}

		return NewCompositeAction(actions), nil
	default:
		return nil, fmt.Errorf("unsupported effect type: %T", effect)
	}
}
