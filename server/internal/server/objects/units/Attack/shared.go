package Attack

type AttackFactory func() IAttack

var MovementRegistry = map[string]AttackFactory{}

func RegisterMovement(name string, factory AttackFactory) {
	MovementRegistry[name] = factory
}

func init() {
	//RegisterMovement("Default Attack", func() IAttack { return &DefaultAttack{} })
}
