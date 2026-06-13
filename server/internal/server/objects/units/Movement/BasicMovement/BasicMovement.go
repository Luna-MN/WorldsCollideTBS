package BasicMovement

import (
	"server/internal/server"
	"server/internal/server/objects"
	"server/pkg/packets"
)

type BasicMovement struct {
	unitId      uint64
	clientId    uint64
	client      *server.Client
	enemyClient *server.Client
}

func (b *BasicMovement) UnitId() uint64 {
	return b.unitId
}

func (b *BasicMovement) ClientId() uint64 {
	return b.clientId
}

func (b *BasicMovement) InitMovement(UnitId uint64, client *server.Client, enemyClient *server.Client) {
	b.unitId = UnitId
	b.clientId = client.Id()
	b.client = client
	b.enemyClient = enemyClient
}

func (b *BasicMovement) Move(infos []*objects.TerrainInfo) {
	//TODO implement me
	panic("implement me")
}

func (b *BasicMovement) SendMovementPacket(infos []*objects.TerrainInfo) {
	positions := make([]objects.Vector3, len(infos))
	for i, info := range infos {
		positions[i] = info.Position
	}
	posPacket := packets.NewHexPositions(b.unitId, positions)
	b.client.SocketSend(posPacket, server.WebSocket)
	b.enemyClient.SocketSend(posPacket, server.WebSocket)
}
