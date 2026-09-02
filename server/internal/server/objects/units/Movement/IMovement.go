package Movement

import (
	"server/internal/server"
	"server/internal/server/objects/units/sharedInterfaces"
	"server/pkg/packets"
)

type IMovement interface {
	UnitId() int32
	ClientId() uint64
	InitMovement(UnitId int32, client *server.Client, enemyClient *server.Client)
	Move(positions []*packets.HexPositionMessage, TerrainService sharedInterfaces.MoveTerrainService, MaxMove int) bool
	SendMovementPacket([]*packets.HexPositionMessage)
}
