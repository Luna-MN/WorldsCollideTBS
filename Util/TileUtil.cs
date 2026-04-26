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

        if (terrainInfo.PreviousTile == null)
        {
            terrainInfo.PreviousTile = terrainInfo.NextTile;
        }

        if (terrainInfo.NextTile == null)
        {
            terrainInfo.NextTile = terrainInfo.PreviousTile;
        }
        int pt = terrainInfo.Neighbours.ToList().IndexOf(terrainInfo.PreviousTile);
        int nt = terrainInfo.Neighbours.ToList().IndexOf(terrainInfo.NextTile);
        if (nt == -1)
        {
            nt = pt;
        }

        if (pt == -1)
        {
            pt = nt;
        }
        int rotationIndex = Math.Min(pt, nt);
        if (tileState == TileState.connection2)
        {
            GD.Print($"connection2 rotation Index: {rotationIndex} for {pt} {nt}");
            GD.Print(IndexToRotation(rotationIndex, tileState));
        }
        return IndexToRotation(rotationIndex, tileState);
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
    public static int IndexToRotation(int index, TileState state)
    {
        if (state == TileState.connection1)
        {
            switch (index)
            {
                case 1: return -180;
                case 2: return -120;
                case 3: return -60;
                case 4: return 0;
                case 5: return 60;
                case 0: return 120;
                default: return 0;
            }
        }
        else if (state == TileState.connection2)
        {
            switch (index)
            {
                case 1: return 240;
                case 2: return 300;
                case 3: return 0;
                case 4: return 60;
                case 5: return 120;
                case 0: return 60;
                default: return 0;
            }
        }
        else if (state == TileState.connection3 || state == TileState.floor)
        {
            switch (index){
                case 1: return 300;
                case 2: return 0;
                case 3: return 60;
                case 4: return 120;
                case 5: return 180;
                case 0: return 240;
                default: return 0;
            }
        }
        return 0;
    }
}