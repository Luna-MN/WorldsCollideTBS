package noise

import "github.com/furui/fastnoiselite-go"

var DefaultNoise = fastnoiselite.FastNoiseLite{
	Seed:                     665,
	Frequency:                0.4296,
	TransformType3D:          fastnoiselite.TransformType3DNone,
	FractalType:              fastnoiselite.FractalTypeFBm,
	Lacunarity:               2,
	WeightedStrength:         0,
	PingPongStrength:         2,
	CellularDistanceFunction: 0,
	CellularReturnType:       0,
	CellularJitterModifier:   1,
	WarpTransformType3D:      fastnoiselite.TransformType3DNone,
	DomainWarpAmp:            30,
}
var PlayerDefaultNoise = fastnoiselite.FastNoiseLite{
	Seed:                     74,
	Frequency:                0.693,
	TransformType3D:          fastnoiselite.TransformType3DNone,
	FractalType:              fastnoiselite.FractalTypeFBm,
	Lacunarity:               2,
	WeightedStrength:         0,
	PingPongStrength:         2,
	CellularDistanceFunction: 0,
	CellularReturnType:       0,
	CellularJitterModifier:   1,
	WarpTransformType3D:      fastnoiselite.TransformType3DNone,
	DomainWarpAmp:            30,
}

var ForestNoise = fastnoiselite.FastNoiseLite{
	Seed:                     74,
	Frequency:                0.1,
	TransformType3D:          fastnoiselite.TransformType3DNone,
	FractalType:              fastnoiselite.FractalTypeFBm,
	Lacunarity:               2,
	WeightedStrength:         0,
	PingPongStrength:         2,
	CellularDistanceFunction: 0,
	CellularReturnType:       0,
	CellularJitterModifier:   1,
	WarpTransformType3D:      fastnoiselite.TransformType3DNone,
	DomainWarpAmp:            30,
}

func init() {
	// default testing noise
	DefaultNoise.SetFractalOctaves(5)
	DefaultNoise.SetFractaclGain(0.5)

	// player noise
	PlayerDefaultNoise.SetFractalOctaves(5)
	PlayerDefaultNoise.SetFractaclGain(0.5)

	// forest noise
	ForestNoise.SetFractalOctaves(5)
	ForestNoise.SetFractaclGain(0.5)
}
