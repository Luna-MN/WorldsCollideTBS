package Attack

import (
	"server/internal/server/objects/units/Skills"
	"server/internal/server/objects/units/Skills/Attack/AttackInstances"
)

func init() {
	Skills.RegisterSkill("Default Attack", func() Skills.ISkill { return &AttackInstances.BasicAttack{} })
}
