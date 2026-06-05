using System;
using System.Collections.Generic;
using Godot;

public class PathFinding
{
    /// <summary>
    /// Finds the shortest weighted path between two terrain tiles using Dijkstra's algorithm
    /// </summary>
    /// <param name="startTile">Starting terrain tile</param>
    /// <param name="endTile">Destination terrain tile</param>
    /// <returns>List of TerrainInfo representing the path, or empty list if no path exists</returns>
    public static List<TerrainInfo> FindCheapestPath(TerrainInfo startTile, TerrainInfo endTile)
    {
        if (startTile == null || endTile == null)
            return new List<TerrainInfo>();

        var openSet = new PriorityQueue<TerrainInfo, int>();
        var cameFrom = new Dictionary<TerrainInfo, TerrainInfo>();
        var gScore = new Dictionary<TerrainInfo, int>();
        var fScore = new Dictionary<TerrainInfo, int>();
        var closedSet = new HashSet<TerrainInfo>();

        gScore[startTile] = 0;
        fScore[startTile] = Heuristic(startTile, endTile);
        GD.Print(fScore.Count);
        openSet.Enqueue(startTile, fScore[startTile]);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            GD.Print(current.Neighbours.Length);
            // Check if we've already processed this tile
            if (closedSet.Contains(current))
                continue;

            if (current == endTile)
                return ReconstructPath(cameFrom, current);

            closedSet.Add(current);

            if (current.Neighbours != null)
            {
                foreach (var neighbor in current.Neighbours)
                {
                    if (neighbor == null || closedSet.Contains(neighbor))
                        continue;

                    // Use 0 if current not in gScore (shouldn't happen, but safe)
                    int currentGScore = gScore.ContainsKey(current) ? gScore[current] : 0;
                    int tentativeGScore = currentGScore + neighbor.MovementCost;

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, endTile);
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }
        }

        GD.Print("No path found between tiles");
        return new List<TerrainInfo>();
    }

    /// <summary>
    /// Heuristic function for A* pathfinding (Manhattan/Chebyshev distance for hexagons)
    /// </summary>
    private static int Heuristic(TerrainInfo from, TerrainInfo to)
    {
        if (from.Position == to.Position)
            return 0;

        // For hexagonal grids, use axial distance
        int dx = Math.Abs(from.PositionI.X - to.PositionI.X);
        int dy = Math.Abs(from.PositionI.Y - to.PositionI.Y);
        return (dx + dy + Math.Abs(dx - dy)) / 2;
    }

    /// <summary>
    /// Reconstructs the path from start to end using the cameFrom dictionary
    /// </summary>
    private static List<TerrainInfo> ReconstructPath(Dictionary<TerrainInfo, TerrainInfo> cameFrom, TerrainInfo current)
    {
        var path = new List<TerrainInfo> { current };
        
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }
}