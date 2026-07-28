package combatDSL

type CombatUnit interface {
	Damage(amount int64)
	Heal(amount int64)

	AddToSkillBuffer(skillID int32, action CombatAction)
	Inflict(skillID int32, action CombatAction, turns int)
	RemoveInflict(skillID int32)
}

type CombatContext struct {
	Caster CombatUnit
	Target CombatUnit
}

type SkillContext struct {
	Context CombatContext
	SkillID int32
	Total   int64
}

type CombatAction interface {
	SkillContexts() map[int32]*SkillContext
	Execute(context CombatContext, skillID int32)
	Invert(skillID int32)
	String() string
}

type UniversalCombatAction struct {
	skillContexts map[int32]*SkillContext
}

func (a *UniversalCombatAction) String() string {
	return "Universal Combat Action"
}

func NewUniversalCombatAction() UniversalCombatAction {
	return UniversalCombatAction{
		skillContexts: make(map[int32]*SkillContext),
	}
}

func (a *UniversalCombatAction) SkillContexts() map[int32]*SkillContext {
	return a.skillContexts
}

func (a *UniversalCombatAction) Execute(context CombatContext, skillID int32) {
	if _, ok := a.skillContexts[skillID]; !ok {
		a.skillContexts[skillID] = &SkillContext{
			Context: context,
			SkillID: skillID,
		}

		context.Caster.AddToSkillBuffer(skillID, a)
	}
}

func (a *UniversalCombatAction) Invert(skillID int32) {}
