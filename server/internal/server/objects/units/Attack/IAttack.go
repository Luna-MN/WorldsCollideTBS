package Attack

import "server/internal/server"

type IAttack interface {
	UnitId() int32
	ClientId() uint64
	InitAttack(UnitId int32, client *server.Client, enemyClient *server.Client)
	Attack(unitId uint64)
	SendAttackPacket(unitId uint64)
}
