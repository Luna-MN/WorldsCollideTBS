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
            // Handle -1 indices by using neighbor of the opposite index
            if (terrainInfo.PreviousTile == null)
            {
                // Use neighbor of nt (next neighbor in clockwise direction)
                PT = (NT + 3) % 6;
            }
            else if (terrainInfo.NextTile == null)
            {
                // Use neighbor of pt (next neighbor in clockwise direction)
                NT = (PT + 3) % 6;
            }

            if (NT == -1 || PT == -1)
            {
                return TileState.connection3;
            }

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
        else if (terrainInfo.Connected == false && terrainInfo.NextTile != null)
        {
            return TileState.connection3;
        }
        return TileState.floor;
    }

    private const int EndSlot = 3;

    public static int GetTileRotation(TerrainInfo terrainInfo, TileState tileState)
    {
        if (tileState != TileState.connection3 && tileState != TileState.connection2 &&
            tileState != TileState.connection1)
        {
            return 0;
        }
        int pt = terrainInfo.Neighbours.ToList().IndexOf(terrainInfo.PreviousTile);
        int nt = terrainInfo.Neighbours.ToList().IndexOf(terrainInfo.NextTile);
        // Handle -1 indices by using neighbor of the opposite index
        if (terrainInfo.PreviousTile == null)
        {
            // Use neighbor of nt (next neighbor in clockwise direction)
            pt = (nt + 3) % 6;
        }
        else if (terrainInfo.NextTile == null)
        {
            // Use neighbor of pt (next neighbor in clockwise direction)
            nt = (pt + 3) % 6;
        }

        if (terrainInfo.PreviousTile == null)
        {
            GD.Print($"index1: {pt}, index2: {nt}");
        }
        var index1 = Math.Min(pt, nt);
        var index2 = Math.Max(pt, nt);
        return IndexToRotation(index1, index2, tileState);
    }
    public static Vector3 WorldToLocal(Vector3 worldPos, Vector3 parentGlobalPos, float parentRotationY)
    {
        // 1. Translate: move the point so the parent is at origin
        Vector3 d = worldPos - parentGlobalPos;

        // 2. Rotate: undo the parent's rotation (note the NEGATIVE angle)
        float cos = Mathf.Cos(-parentRotationY);
        float sin = Mathf.Sin(-parentRotationY);

        return new Vector3(
            d.X * cos - d.Z * sin,
            d.Y,
            d.X * sin + d.Z * cos
        );
    }

    public static int IndexToRotation(int index1, int index2, TileState state)
    {
        switch (state, index1, index2)
        {
            case (TileState.connection2, 1, 5):
                return 120;
            case (TileState.connection2, 0, 2):
                return 180;
            case (TileState.connection2, 1, 3):
                return -120;
            case (TileState.connection2, 2, 4):
                return -60;
            case (TileState.connection2, 3, 5):
                return 0;
            case (TileState.connection2, 0, 4):
                return 60;
            case (TileState.connection1, 0, 1):
                return 120;
            case (TileState.connection1, 1, 2):
                return 180;
            case (TileState.connection1, 2, 3):
                return -120;
            case (TileState.connection1, 3, 4):
                return -60;
            case (TileState.connection1, 4, 5):
                return 0;
            case (TileState.connection1, 0, 5):
                return 60;
            case (TileState.connection3, 1, 4):
                return -60;
            case (TileState.connection3, 2, 5):
                return 0;
            case (TileState.connection3, 0, 3):
                return 60;
            default:
                GD.Print($"Invalid {state}, {index1}, {index2} combination");
                return 0;
        }
    }
}