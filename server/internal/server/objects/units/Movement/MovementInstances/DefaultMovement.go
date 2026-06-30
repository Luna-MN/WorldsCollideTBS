package MovementInstances

import (
	"server/internal/server"
	"server/pkg/packets"
)

type DefaultMovement struct {
	unitId      int32
	clientId    uint64
	client      *server.Client
	enemyClient *server.Client
}

func (d *DefaultMovement) UnitId() int32 {
	return d.unitId
}

func (d *DefaultMovement) ClientId() uint64 {
	return d.clientId
}

func (d *DefaultMovement) InitMovement(UnitId int32, client *server.Client, enemyClient *server.Client) {
	d.unitId = UnitId
	d.clientId = client.Id()
	d.client = client
	d.enemyClient = enemyClient
}

func (d *DefaultMovement) Move(positions []*packets.HexPositionMessage) bool {
	// make sure the movement is valid

	d.SendMovementPacket(positions)

	return true // is movement valid
}

func (d *DefaultMovement) SendMovementPacket(positions []*packets.HexPositionMessage) {
	posPacket := packets.NewHexPositions(d.unitId, positions)
	d.client.SocketSend(posPacket, server.WebSocket)
	d.enemyClient.SocketSendAs(posPacket, d.client.Id(), server.WebSocket)
}
