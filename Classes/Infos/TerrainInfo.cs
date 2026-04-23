using System;
using Godot;
[Tool]
public class TerrainInfo
{
    public TileUtil.TileType TileType = TileUtil.TileType.Grass;
    public Vector3 Position;
    public TerrainInfo[] Neighbours;
    public bool Connected;
    public TerrainInfo NextTile;
    public TerrainInfo PreviousTile;
    public int TileHeight;
}