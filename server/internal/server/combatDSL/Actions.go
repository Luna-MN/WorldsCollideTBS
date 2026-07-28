package combatDSL

import "fmt"

type DamageAction struct {
	UniversalCombatAction
	Amount int64
}

func NewDamageAction(amount int) *DamageAction {
	return &DamageAction{
		UniversalCombatAction: NewUniversalCombatAction(),
		Amount:                int64(amount),
	}
}

func (a *DamageAction) Execute(context CombatContext, skillID int32) {
	a.UniversalCombatAction.Execute(context, skillID)

	skillContext := a.SkillContexts()[skillID]
	skillContext.Total += a.Amount
	skillContext.Context.Target.Damage(a.Amount)
}

func (a *DamageAction) Invert(skillID int32) {
	skillContext, ok := a.SkillContexts()[skillID]
	if !ok {
		return
	}

	skillContext.Context.Target.Heal(skillContext.Total)
	delete(a.SkillContexts(), skillID)
}

func (a *DamageAction) String() string {
	return fmt.Sprintf("Damage {Amount: %d}", a.Amount)
}

type HealAction struct {
	UniversalCombatAction
	Amount int64
}

func NewHealAction(amount int) *HealAction {
	return &HealAction{
		UniversalCombatAction: NewUniversalCombatAction(),
		Amount:                int64(amount),
	}
}

func (a *HealAction) Execute(context CombatContext, skillID int32) {
	a.UniversalCombatAction.Execute(context, skillID)

	skillContext := a.SkillContexts()[skillID]
	skillContext.Total += a.Amount
	skillContext.Context.Target.Heal(a.Amount)
}

func (a *HealAction) Invert(skillID int32) {
	skillContext, ok := a.SkillContexts()[skillID]
	if !ok {
		return
	}

	skillContext.Context.Target.Damage(skillContext.Total)
	delete(a.SkillContexts(), skillID)
}

func (a *HealAction) String() string {
	return fmt.Sprintf("Heal {Amount: %d}", a.Amount)
}

type BleedAction struct {
	UniversalCombatAction
	Amount int64
}

func NewBleedAction(amount int) *BleedAction {
	return &BleedAction{
		UniversalCombatAction: NewUniversalCombatAction(),
		Amount:                int64(amount),
	}
}

func (a *BleedAction) Execute(context CombatContext, skillID int32) {
	a.UniversalCombatAction.Execute(context, skillID)

	skillContext := a.SkillContexts()[skillID]
	skillContext.Total += a.Amount
	skillContext.Context.Target.Damage(a.Amount)
}

func (a *BleedAction) Invert(skillID int32) {
	skillContext, ok := a.SkillContexts()[skillID]
	if !ok {
		return
	}

	skillContext.Context.Target.Heal(skillContext.Total)
	delete(a.SkillContexts(), skillID)
}

func (a *BleedAction) String() string {
	return fmt.Sprintf("Bleed {Amount: %d}", a.Amount)
}

type PoisonAction struct {
	UniversalCombatAction
	Amount int64
}

func NewPoisonAction(amount int) *PoisonAction {
	return &PoisonAction{
		UniversalCombatAction: NewUniversalCombatAction(),
		Amount:                int64(amount),
	}
}

func (a *PoisonAction) Execute(context CombatContext, skillID int32) {
	a.UniversalCombatAction.Execute(context, skillID)

	skillContext := a.SkillContexts()[skillID]
	skillContext.Total += a.Amount

	// TODO: replace with poison-specific behavior once status effects are wired.
	skillContext.Context.Target.Damage(a.Amount)
}

func (a *PoisonAction) Invert(skillID int32) {
	skillContext, ok := a.SkillContexts()[skillID]
	if !ok {
		return
	}

	skillContext.Context.Target.Heal(skillContext.Total)
	delete(a.SkillContexts(), skillID)
}

func (a *PoisonAction) String() string {
	return fmt.Sprintf("Poison {Amount: %d}", a.Amount)
}

type CompositeAction struct {
	UniversalCombatAction
	Actions []CombatAction
}

func NewCompositeAction(actions []CombatAction) *CompositeAction {
	return &CompositeAction{
		UniversalCombatAction: NewUniversalCombatAction(),
		Actions:               actions,
	}
}

func (a *CompositeAction) Execute(context CombatContext, skillID int32) {
	for _, action := range a.Actions {
		action.Execute(context, skillID)
	}
}

func (a *CompositeAction) Invert(skillID int32) {
	for _, action := range a.Actions {
		action.Invert(skillID)
	}
}

func (a *CompositeAction) String() string {
	actions := make([]string, len(a.Actions))
	for i, action := range a.Actions {
		actions[i] = action.String()
	}
	return fmt.Sprintf("Composite {%s}", actions)
}

type InflictAction struct {
	UniversalCombatAction
	Effect CombatAction
	Turns  int
}

func NewInflictAction(effect CombatAction, turns int) *InflictAction {
	return &InflictAction{
		UniversalCombatAction: NewUniversalCombatAction(),
		Effect:                effect,
		Turns:                 turns,
	}
}

func (a *InflictAction) Execute(context CombatContext, skillID int32) {
	a.UniversalCombatAction.Execute(context, skillID)
	context.Target.Inflict(skillID, a.Effect, a.Turns)
}

func (a *InflictAction) Invert(skillID int32) {
	skillContext, ok := a.SkillContexts()[skillID]
	if !ok {
		return
	}

	skillContext.Context.Caster.RemoveInflict(skillID)
	delete(a.SkillContexts(), skillID)
}

func (a *InflictAction) String() string {
	effect := a.Effect.String()
	return fmt.Sprintf("Inflict {Effect: %s, Turns: %d}", effect, a.Turns)
}

type DelayAction struct {
	UniversalCombatAction
	Turns int
	Then  FuncExpr
}

func NewDelayAction(turns int, then FuncExpr) *DelayAction {
	return &DelayAction{
		UniversalCombatAction: NewUniversalCombatAction(),
		Turns:                 turns,
		Then:                  then,
	}
}

func (a *DelayAction) Execute(context CombatContext, skillID int32) {
	// TODO: queue this with your turn-management service.
}

func (a *DelayAction) Invert(skillID int32) {
	// TODO: cancel delayed queued action.
}

func (a *DelayAction) String() string {
	// TODO: implement me
	return "Delay"
}
