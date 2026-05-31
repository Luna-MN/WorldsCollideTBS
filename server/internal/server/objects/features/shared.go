package features

import "server/internal/server/objects"

func init() {
	objects.RegisterFeature("Forest", func() objects.IFeature {
		return &Forest{}
	})
	objects.RegisterFeature("River", func() objects.IFeature {
		return &River{}
	})
}
