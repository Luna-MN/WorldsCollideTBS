package objects

import (
	"math"
)

type Vector2 struct {
	X float32
	Y float32
}

// NewVector2 Create a new vector
func NewVector2(X float32, Y float32) *Vector2 {
	return &Vector2{X, Y}
}

// Add two vector 2's together and return a new vector'
func (v *Vector2) Add(v2 *Vector2) *Vector2 {
	return &Vector2{v.X + v2.X, v.Y + v2.Y}
}

// Sub two vector 2's together and return a new vector
func (v *Vector2) Sub(v2 *Vector2) *Vector2 {
	return &Vector2{v.X - v2.X, v.Y - v2.Y}
}

// DistanceSquaredTo returns the squared distance between two vectors
func (v *Vector2) DistanceSquaredTo(v2 *Vector2) float32 {
	return (v.X-v2.X)*(v.X-v2.X) + (v.Y-v2.Y)*(v.Y-v2.Y)
}

// DistanceTo returns the distance between two vectors
func (v *Vector2) DistanceTo(v2 *Vector2) float32 {
	return float32(math.Sqrt(float64(v.DistanceSquaredTo(v2))))
}

type Vector3 struct {
	X, Y, Z float32
}

// NewVector3 Create a new vector
func NewVector3(X float32, Y float32, Z float32) Vector3 {
	return Vector3{X, Y, Z}
}

// Add two vector 3's together and return a new vector
func (v *Vector3) Add(v2 *Vector3) *Vector3 {
	return &Vector3{v.X + v2.X, v.Y + v2.Y, v.Z + v2.Z}
}

// Sub two vector 3's together and return a new vector
func (v *Vector3) Sub(v2 *Vector3) *Vector3 {
	return &Vector3{v.X - v2.X, v.Y - v2.Y, v.Z - v2.Z}
}

// DistanceSquaredTo returns the squared distance between two vectors
func (v *Vector3) DistanceSquaredTo(v2 *Vector3) float32 {
	return (v.X-v2.X)*(v.X-v2.X) + (v.Y-v2.Y)*(v.Y-v2.Y) + (v.Z-v2.Z)*(v.Z-v2.Z)
}

// DistanceTo returns the distance between two vectors
func (v *Vector3) DistanceTo(v2 *Vector3) float32 {
	return float32(math.Sqrt(float64(v.DistanceSquaredTo(v2))))
}
