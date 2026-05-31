package objects

type FeatureFactory func() IFeature

var featureRegistry = map[string]FeatureFactory{}

func RegisterFeature(name string, factory FeatureFactory) {
	featureRegistry[name] = factory
}
