package PathFinding

import (
	"container/heap"
	"fmt"

	"server/internal/server/objects/tiles"
)

type pathNode struct {
	tile     *tiles.TerrainInfo
	priority int
	index    int
}

type priorityQueue []*pathNode

func (pq priorityQueue) Len() int {
	return len(pq)
}

func (pq priorityQueue) Less(i, j int) bool {
	return pq[i].priority < pq[j].priority
}

func (pq priorityQueue) Swap(i, j int) {
	pq[i], pq[j] = pq[j], pq[i]
	pq[i].index = i
	pq[j].index = j
}

func (pq *priorityQueue) Push(x any) {
	node := x.(*pathNode)
	node.index = len(*pq)
	*pq = append(*pq, node)
}

func (pq *priorityQueue) Pop() any {
	old := *pq
	n := len(old)

	node := old[n-1]
	old[n-1] = nil
	node.index = -1

	*pq = old[:n-1]
	return node
}

// FindCheapestPath finds the cheapest weighted path between two terrain tiles using A* pathfinding.
// It returns a slice of TerrainInfo pointers representing the path, or an empty slice if no path exists.
func FindCheapestPath(startTile, endTile *tiles.TerrainInfo) []*tiles.TerrainInfo {
	if startTile == nil || endTile == nil {
		return []*tiles.TerrainInfo{}
	}

	openSet := &priorityQueue{}
	heap.Init(openSet)

	cameFrom := make(map[*tiles.TerrainInfo]*tiles.TerrainInfo)
	gScore := make(map[*tiles.TerrainInfo]int)
	fScore := make(map[*tiles.TerrainInfo]int)
	closedSet := make(map[*tiles.TerrainInfo]bool)

	gScore[startTile] = 0
	fScore[startTile] = heuristic(startTile, endTile)

	fmt.Println(len(fScore))

	heap.Push(openSet, &pathNode{
		tile:     startTile,
		priority: fScore[startTile],
	})

	for openSet.Len() > 0 {
		current := heap.Pop(openSet).(*pathNode).tile

		fmt.Println(len(current.Neighbors))

		if closedSet[current] {
			continue
		}

		if current == endTile {
			return reconstructPath(cameFrom, current)
		}

		closedSet[current] = true

		for _, neighbor := range current.Neighbors {
			if neighbor == nil || closedSet[neighbor] {
				continue
			}

			currentGScore, ok := gScore[current]
			if !ok {
				currentGScore = 0
			}

			tentativeGScore := currentGScore + neighbor.CalculateMovementCost(current)

			neighborGScore, exists := gScore[neighbor]
			if !exists || tentativeGScore < neighborGScore {
				cameFrom[neighbor] = current
				gScore[neighbor] = tentativeGScore
				fScore[neighbor] = tentativeGScore + heuristic(neighbor, endTile)

				heap.Push(openSet, &pathNode{
					tile:     neighbor,
					priority: fScore[neighbor],
				})
			}
		}
	}

	fmt.Println("No path found between tiles")
	return []*tiles.TerrainInfo{}
}

// heuristic estimates the distance between two hex tiles using axial hex distance.
func heuristic(from, to *tiles.TerrainInfo) int {
	if from.Position == to.Position {
		return 0
	}

	dx := absInt(from.PositionI.X - to.PositionI.X)
	dy := absInt(from.PositionI.Y - to.PositionI.Y)

	return (dx + dy + absInt(dx-dy)) / 2
}

// reconstructPath rebuilds the path from start to end using the cameFrom map.
func reconstructPath(
	cameFrom map[*tiles.TerrainInfo]*tiles.TerrainInfo,
	current *tiles.TerrainInfo,
) []*tiles.TerrainInfo {
	path := []*tiles.TerrainInfo{current}

	for {
		previous, ok := cameFrom[current]
		if !ok {
			break
		}

		current = previous
		path = append([]*tiles.TerrainInfo{current}, path...)
	}

	return path
}

func absInt(value int) int {
	if value < 0 {
		return -value
	}

	return value
}
