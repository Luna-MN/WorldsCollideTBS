package Attack

import "server/internal/server"

type IAttack interface {
	UnitId() uint64
	ClientId() uint64
	InitAttack(UnitId uint64, client *server.Client, enemyClient *server.Client)
	Attack(unitId uint64)
	SendAttackPacket(unitId uint64)
}
