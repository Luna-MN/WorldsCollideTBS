package util

import "server/internal/server/objects"

type Speed uint32

const (
	Normal Speed = iota
	Fast
	Slow
)

type UnitData struct {
	UnitName string
	OwnerID  uint64
	UnitID   uint64
	Attacks  []string
	Movement string
	AP       int32
	HP       int32
	MaxHP    int32
	Pos      objects.Vector3
	Speed    Speed
}

func NewUnitData(unitName string, ownerID uint64, unitID uint64, attacks []string, movement string, ap, hp, maxHP int32, speed Speed, pos objects.Vector3) *UnitData {
	return &UnitData{
		UnitName: unitName,
		OwnerID:  ownerID,
		UnitID:   unitID,
		Attacks:  attacks,
		Movement: movement,
		AP:       ap,
		HP:       hp,
		MaxHP:    maxHP,
		Speed:    speed,
		Pos:      pos,
	}
}
