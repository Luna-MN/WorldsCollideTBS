package Movement

import "server/internal/server/objects/units/Movement/MovementInstances"

type MovementFactory func() IMovement

var MovementRegistry = map[string]MovementFactory{}

func RegisterMovement(name string, factory MovementFactory) {
	MovementRegistry[name] = factory
}

func init() {
	RegisterMovement("Default Movement", func() IMovement { return &MovementInstances.DefaultMovement{} })
}
