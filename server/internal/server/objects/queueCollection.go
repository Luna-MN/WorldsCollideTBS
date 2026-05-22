package objects

import (
	"math"
	"server/internal"
	"sync"
)

type QueueClient struct {
	Id        uint64
	ClientId  uint64
	ClientMMR uint64
}

func NewQueueClient(id, clientId, clientMMR uint64) QueueClient {
	return QueueClient{
		Id:        id,
		ClientId:  clientId,
		ClientMMR: clientMMR,
	}
}

type QueueCollection[q QueueClient] struct {
	objectMap map[uint64]QueueClient // maps id to a client
	nextId    uint64
	mapMux    sync.RWMutex
}

func NewQueueCollection[q QueueClient](capacity ...int) *QueueCollection[q] {
	var newObjMap map[uint64]QueueClient

	if len(capacity) > 0 {
		newObjMap = make(map[uint64]QueueClient, capacity[0])
	} else {
		newObjMap = make(map[uint64]QueueClient)
	}
	return &QueueCollection[q]{objectMap: newObjMap, nextId: 1}
}
func (qc *QueueCollection[q]) GetNextId() uint64 {
	qc.mapMux.Lock()
	defer qc.mapMux.Unlock()
	return qc.nextId
}
func (qc *QueueCollection[q]) Add(client QueueClient, id uint64) uint64 {
	qc.mapMux.Lock()
	defer qc.mapMux.Unlock()

	thisId := id

	qc.objectMap[thisId] = client
	qc.nextId++
	return thisId
}

func (qc *QueueCollection[q]) Remove(id uint64) {
	qc.mapMux.Lock()
	defer qc.mapMux.Unlock()

	delete(qc.objectMap, id)
}

func (qc *QueueCollection[q]) Get(id uint64) (*QueueClient, bool) {
	qc.mapMux.RLock()
	defer qc.mapMux.RUnlock()
	c, ok := qc.objectMap[id]
	return &c, ok
}

func (qc *QueueCollection[q]) Len() int {
	qc.mapMux.RLock()
	defer qc.mapMux.RUnlock()
	return len(qc.objectMap)
}

func (qc *QueueCollection[q]) ForEach(callback func(id uint64, obj QueueClient)) {
	qc.mapMux.Lock()
	localMap := make(map[uint64]QueueClient, len(qc.objectMap))
	for id, obj := range qc.objectMap {
		localMap[id] = obj
	}
	qc.mapMux.Unlock()

	for id, obj := range localMap {
		callback(id, obj)
	}
}

func (qc *QueueCollection[q]) Clear() {
	qc.mapMux.Lock()
	qc.objectMap = make(map[uint64]QueueClient)
	qc.mapMux.Unlock()
}

func (qc *QueueCollection[q]) WithinMMRBounds(mmr uint64) map[uint64]QueueClient {
	qc.mapMux.RLock()
	defer qc.mapMux.RUnlock()

	result := make(map[uint64]QueueClient)
	for id, obj := range qc.objectMap {
		objMMR := obj.ClientMMR
		lower := uint64(0)              // fallback to stop underflow
		upper := uint64(math.MaxUint64) // fallback to stop overflow

		if mmr > internal.MMRRange {
			lower = mmr - internal.MMRRange
		}
		if math.MaxUint64-mmr >= internal.MMRRange {
			upper = mmr + internal.MMRRange
		}

		if objMMR >= lower && objMMR <= upper {
			result[id] = obj
		}
	}
	return result
}

func (qc *QueueCollection[q]) IdInQueue(id uint64) bool {
	qc.mapMux.RLock()
	defer qc.mapMux.RUnlock()
	if _, ok := qc.objectMap[id]; ok {
		return ok
	}
	return false
}
