using System;
using System.Collections.Generic;
using Godot;
[Tool]
public class TerrainInfo
{
    public TileUtil.TileType TileType;
    public TileUtil.TileTopType TileTopType = TileUtil.TileTopType.None;
    public TileUtil.TileTopState TopTileState = TileUtil.TileTopState.Small;
    public Vector2 Position;
    public Vector2I PositionI;
    public Vector2I PositionL;
    public int z;
    public TerrainInfo[] Neighbours;
    public Dictionary< TerrainInfo, TileUtil.TileConnection> Connections;
    public bool Connected;
    public TerrainInfo NextTile;
    public TerrainInfo PreviousTile;
    public Tile tile;
    public int TileHeight;
    public IUnit Unit;
    public int MovementCost;
    public int CalculatedMovementCost(TerrainInfo FromTile)
    {
        // cost based on type
        switch (TileTopType)
        {
            case TileUtil.TileTopType.Tree:
                MovementCost = 2;
                break;
            case TileUtil.TileTopType.None:
                MovementCost = 1;
                break;
            case TileUtil.TileTopType.Stone:
                MovementCost = 3;
                break;
        }
        
        // height cost
        var dif = FromTile.TileHeight - TileHeight;
        dif = Mathf.Max(dif, -1);
        MovementCost += dif;
        
        return MovementCost;
    }
}