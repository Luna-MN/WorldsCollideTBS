package services

type TurnManagementService struct {
	turnIds     []uint64
	turnPointer uint64
	player1Id   uint64
	player2Id   uint64
}

func NewTurnManagementService(player1Id uint64, player2Id uint64) *TurnManagementService {
	return &TurnManagementService{turnIds: []uint64{player1Id, player2Id}, player1Id: player1Id, player2Id: player2Id, turnPointer: 0}
}

func (t *TurnManagementService) SetPlayers(player1Id uint64, player2Id uint64) {
	t.player1Id = player1Id
	t.player2Id = player2Id
}

func (t *TurnManagementService) Name() string {
	return "TurnManagementService"
}

func (t *TurnManagementService) Id() uint64 {
	return 0
}

func (t *TurnManagementService) Turn() uint64 {
	return t.turnIds[t.turnPointer]
}

func (t *TurnManagementService) NextTurn() uint64 {
	t.turnPointer++
	if t.turnPointer >= uint64(len(t.turnIds)) {
		t.turnPointer = 0
	}
	return t.turnIds[t.turnPointer]
}

func (t *TurnManagementService) AddToTurnOrder(id uint64) {
	t.turnIds = append(t.turnIds, id)
}

func (t *TurnManagementService) RemoveFromTurnOrder(id uint64) {
	for i, turnId := range t.turnIds {
		if turnId == id {
			t.turnIds = append(t.turnIds[:i], t.turnIds[i+1:]...)
			break
		}
	}
}

func (t *TurnManagementService) GetTurnOrder() []uint64 {
	return t.turnIds
}

func (t *TurnManagementService) IsMyTurn(id uint64) bool {
	return t.turnIds[t.turnPointer] == id
}

func (t *TurnManagementService) IsPlayerTurn(id uint64) bool {
	return t.player1Id == id || t.player2Id == id
}
