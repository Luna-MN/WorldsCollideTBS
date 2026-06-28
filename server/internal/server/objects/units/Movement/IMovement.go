package Movement

import (
	"server/internal/server"
	"server/pkg/packets"
)

type IMovement interface {
	UnitId() int32
	ClientId() uint64
	InitMovement(UnitId int32, client *server.Client, enemyClient *server.Client)
	Move([]*packets.HexPositionMessage)
	SendMovementPacket([]*packets.HexPositionMessage)
}
