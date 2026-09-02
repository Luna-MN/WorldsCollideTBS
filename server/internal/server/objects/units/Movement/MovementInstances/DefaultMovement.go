package MovementInstances

import (
	"fmt"
	"server/internal/server"
	"server/internal/server/objects/tiles"
	"server/internal/server/objects/units/sharedInterfaces"
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

func (d *DefaultMovement) Move(positions []*packets.HexPositionMessage, TerrainService sharedInterfaces.MoveTerrainService, MaxMove int) bool {
	// make sure the movement is valid
	if len(positions) == 0 {
		return false
	}

	if TerrainService == nil {
		return false
	}

	terrainTiles := make([]*tiles.TerrainInfo, len(positions))
	for i, pos := range positions {
		if pos == nil {
			fmt.Println("movement denied: nil HexPositionMessage at index", i)
			return false
		}

		if pos.Position == nil {
			fmt.Println("movement denied: nil Position at index", i)
			return false
		}

		tilePos := packets.UnwrapVector2I(pos.Position)

		tile := TerrainService.GetTileAt(tilePos)
		if tile == nil {
			fmt.Println("movement denied: no terrain tile at", tilePos)
			return false
		}

		terrainTiles[i] = tile
	}
	moveCost := 0
	for i, tile := range terrainTiles {
		if i == 0 {
			continue
		}

		fromTile := terrainTiles[i-1]
		if tile == nil || fromTile == nil {
			return false
		}

		moveCost += tile.CalculateMovementCost(fromTile)
	}

	if moveCost > MaxMove {
		return false
	}

	d.SendMovementPacket(positions)
	return true // is movement valid
}
func (d *DefaultMovement) SendMovementPacket(positions []*packets.HexPositionMessage) {
	if d.client == nil || d.enemyClient == nil {
		return
	}

	posPacket := packets.NewHexPositions(d.unitId, positions)
	d.client.SocketSend(posPacket, server.WebSocket)
	d.enemyClient.SocketSendAs(posPacket, d.client.Id(), server.WebSocket)
}
