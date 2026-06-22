package util

import (
	"server/internal/server/db"
	"server/internal/server/objects"
	"server/pkg/packets"
	"strings"
)

type Speed uint32

const (
	Normal Speed = iota
	Fast
	Slow
)

type UnitData struct {
	UnitName string
	OwnerID  uint64
	UnitID   int64
	Attacks  []string
	Movement string
	AP       int64
	HP       int64
	MaxHP    int64
	Pos      objects.Vector3
	Speed    Speed
}

func NewUnitData(unitName string, ownerID uint64, unitID int64, attacks []string, movement string, ap, hp, maxHP int64, speed Speed, pos objects.Vector3) *UnitData {
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

func NewUnitDataFromDB(unit *db.Unit, ownerID uint64) *UnitData {
	return &UnitData{
		UnitName: unit.Name,
		OwnerID:  ownerID,
		UnitID:   unit.ID,
		Attacks:  strings.Split(unit.Attacks.String, ","),
		Movement: unit.Movement.String,
		AP:       unit.Ap.Int64,
		HP:       unit.Maxhp.Int64,
		MaxHP:    unit.Maxhp.Int64,
		Speed:    Speed(unit.Speed.Int64),
	}
}

func (u *UnitData) UpdateUnit(UnitMessage *packets.UnitMessage) {
	u.OwnerID = UnitMessage.OwnerId
	u.UnitID = UnitMessage.UnitId
	u.AP = UnitMessage.AP
	u.HP = UnitMessage.HP
	u.MaxHP = UnitMessage.MaxHP
	u.Speed = Speed(UnitMessage.Speed)
	u.Pos = objects.NewVector3(UnitMessage.Pos.X, UnitMessage.Pos.Y, UnitMessage.Pos.Z)
}
