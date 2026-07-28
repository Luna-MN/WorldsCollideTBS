package features

import (
	"server/internal/server/objects/tiles"
)

func init() {
	tiles.RegisterFeature("Forest", func() tiles.IFeature {
		return &Forest{}
	})
	tiles.RegisterFeature("River", func() tiles.IFeature {
		return &River{}
	})
}
