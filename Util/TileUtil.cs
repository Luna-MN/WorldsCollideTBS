using Godot;
using System;
using System.Linq;

public static class TileUtil
{
    public enum TileState
    {
        floor,
        underground,
        connection1,
        connection2,
        connection3,
    }

    public enum TileType
    {
        Grass,
        River,
        Path
    }

    public static TileState GetState(TerrainInfo terrainInfo, int y)
    {
        if (terrainInfo.TileHeight != y)
        {
            return TileState.underground;
        }
        if (terrainInfo.Connected)
        {
            var PT = terrainInfo.Neighbours.ToList().IndexOf(terrainInfo.PreviousTile);
            var NT = terrainInfo.Neighbours.ToList().IndexOf(terrainInfo.NextTile);
            var dif = Math.Abs(PT - NT);
            if (dif == 1 || dif == 5)
            {
                return TileState.connection1;
            }

            if (dif == 2 || dif == 4)
            {
                return TileState.connection2;
            }

            if (dif == 3)
            {
                return TileState.connection3;
            }
        }

        return TileState.floor;
    }
}