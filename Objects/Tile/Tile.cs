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
    private TerrainInfo TerrainInfo;
    private float X;
    private float Z;
    
    public Tile() { }
    public Tile(TileMap tileMap, float x, float z, TerrainInfo TI = null)
    {
        TileMap = tileMap;
        X = x;
        Z = z;
        TerrainInfo = TI;
    }

    public void Set(TileMap tileMap, float x, float z, TerrainInfo TI = null)
    {
        TileMap = tileMap;
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
        StaticBody.Position = new Vector3(0, TerrainInfo.TileHeight, 0);
    }
}
