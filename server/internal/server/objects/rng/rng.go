package rng

type SplitMix64 struct {
	state uint64
}

func NewSplitMix64(seed uint64) *SplitMix64 {
	return &SplitMix64{state: seed}
}

func (r *SplitMix64) Uint64() uint64 {
	r.state += 0x9E3779B97F4A7C15

	z := r.state
	z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9
	z = (z ^ (z >> 27)) * 0x94D049BB133111EB
	return z ^ (z >> 31)
}

func (r *SplitMix64) IntN(n int) int {
	if n <= 0 {
		panic("n must be positive")
	}

	return int(r.Uint64() % uint64(n))
}

func (r *SplitMix64) Float64() float64 {
	return float64(r.Uint64()>>11) * (1.0 / 9007199254740992.0)
}
