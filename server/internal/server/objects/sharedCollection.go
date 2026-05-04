package objects

import "sync"

// SharedCollection A generic, thread-safe map of objects with IDs
type SharedCollection[t any] struct {
	objectMap map[uint64]t
	nextId    uint64
	mapMux    sync.Mutex
}

func NewSharedCollection[t any](capacity ...int) *SharedCollection[t] {
	var newObjMap map[uint64]t

	if len(capacity) > 0 {
		newObjMap = make(map[uint64]t, capacity[0])
	} else {
		newObjMap = make(map[uint64]t)
	}
	return &SharedCollection[t]{objectMap: newObjMap, nextId: 1}
}

// Add an object to the map with given ID (if provided) or next available ID
// Returns the ID of the added object
func (sc *SharedCollection[t]) Add(obj t, id ...uint64) uint64 {
	sc.mapMux.Lock()
	defer sc.mapMux.Unlock()

	thisId := sc.nextId
	if len(id) > 0 {
		thisId = id[0]
	}
	sc.objectMap[thisId] = obj
	sc.nextId++
	return thisId
}

// Remove removes an object from the map by ID
func (sc *SharedCollection[t]) Remove(id uint64) {
	sc.mapMux.Lock()
	defer sc.mapMux.Unlock()

	delete(sc.objectMap, id)
}

// ForEach Call the callback function for each object in the map
func (sc *SharedCollection[t]) ForEach(callback func(id uint64, obj t)) {
	sc.mapMux.Lock()
	localMap := make(map[uint64]t, len(sc.objectMap))
	for id, obj := range sc.objectMap {
		localMap[id] = obj
	}
	sc.mapMux.Unlock()

	for id, obj := range localMap {
		callback(id, obj)
	}
}

// Get an object from the map by ID
// if object doesn't exist, returns false and nil
func (sc *SharedCollection[t]) Get(id uint64) (t, bool) {
	sc.mapMux.Lock()
	defer sc.mapMux.Unlock()

	obj, ok := sc.objectMap[id]
	return obj, ok
}

// Len Get the number of objects in the map
func (sc *SharedCollection[t]) Len() int {
	sc.mapMux.Lock()
	defer sc.mapMux.Unlock()
	return len(sc.objectMap)
}
