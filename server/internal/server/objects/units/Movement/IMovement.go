package Movement

import (
	"server/internal/server"
	"server/internal/server/objects"
)

type IMovement interface {
	UnitId() uint64
	ClientId() uint64
	InitMovement(UnitId uint64, client *server.Client, enemyClient *server.Client)
	Move([]*objects.TerrainInfo)
	SendMovementPacket([]*objects.TerrainInfo)
}
