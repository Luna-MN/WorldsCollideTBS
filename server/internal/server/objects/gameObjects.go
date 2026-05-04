package objects

type Player struct {
	Name  string
	Owner uint64

	Level        uint64
	Position     Vector3
	Direction    Vector3
	Speed        float32
	Rotation     Vector3
	SentRotation Vector3
}
