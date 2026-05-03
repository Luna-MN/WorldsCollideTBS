using System;
using Godot;
[Tool]
public class TerrainInfo
{
    public TileUtil.TileType TileType;
    public TileUtil.TileTopType TileTopType = TileUtil.TileTopType.None;
    public TileUtil.TileTopState TopTileState = TileUtil.TileTopState.Small;
    public Vector3 Position;
    public Vector2I PositionI;
    public TerrainInfo[] Neighbours;
    public bool Connected;
    public TerrainInfo NextTile;
    public TerrainInfo PreviousTile;
    public int TileHeight;
}