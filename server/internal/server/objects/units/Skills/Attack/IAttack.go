package Attack

import (
	"server/internal/server/objects"
	"server/internal/server/objects/units/Skills"
)

type IAttack interface {
	Skills.ISkill

	Attack(pos objects.Vector3)
	AttackValidation(pos objects.Vector3)
	SendAttackPacket(pos objects.Vector3)
}
