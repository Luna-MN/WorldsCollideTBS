using Godot;
using System;
[Tool]
public partial class Tile : Node3D
{
    private Node3D[] TileNodes;
    
    [Export] 
    private Node3D NodeParent;
    [Export]
    private StaticBody3D StaticBody;
    private TileMap TileMap;
    private TopTileMap TopTileMap;
    private TerrainInfo TerrainInfo;
    private float X;
    private float Z;
    
    public Tile() { }
    public Tile(TileMap tileMap, TopTileMap topTileMap, float x, float z, TerrainInfo TI = null)
    {
        TileMap = tileMap;
        TopTileMap = topTileMap;
        X = x;
        Z = z;
        TerrainInfo = TI;
    }

    public void Set(TileMap tileMap, TopTileMap topTileMap, float x, float z, TerrainInfo TI = null)
    {
        TileMap = tileMap;
        TopTileMap = topTileMap;
        X = x;
        Z = z;
        TerrainInfo = TI;
    }
    public void GenerateTile()
    {
        Position = new Vector3(X, 0, Z);
        PackedScene PS = null;
        for (int i = 0; i <= TerrainInfo.TileHeight; i++)
        {
            PS = TileMap[TerrainInfo, i].TileNode;

            var node = PS.Instantiate<Node3D> ();
            AddChild(node);
            node.Position = new Vector3(0, i, 0);
            node.RotationDegrees = new Vector3(0, TileUtil.GetTileRotation(TerrainInfo, TileUtil.GetState(TerrainInfo, i)), 0);
        }
        if (TerrainInfo.TileTopType != TileUtil.TileTopType.None && TerrainInfo.TileType == TileUtil.TileType.Grass)
        {
            var TPS = TopTileMap[TerrainInfo.TopTileState].TileNode;
            var topNode = TPS.Instantiate<Node3D>();
            AddChild(topNode);
            topNode.Position = new Vector3(0, TerrainInfo.TileHeight, 0);
        }
        StaticBody.Position = new Vector3(0, TerrainInfo.TileHeight, 0);
    }
}
