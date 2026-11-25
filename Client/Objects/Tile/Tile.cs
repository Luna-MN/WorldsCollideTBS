using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class Tile : Node2D
{
    
    [Export] 
    private Node2D NodeParent;
    [Export]
    public StaticBody2D StaticBody;
    [Export]
    private Material SelectMaterial;
    private Material ActualMaterial;
    private TileMap TileMap;
    private TopTileMap TopTileMap;
    public TerrainInfo TerrainInfo;
    public float OrgHeightOffset;
    public float HeightOffset;
    private float heightOffset;
    private float Adjust = 0.5f;
    private float X;
    private float Z;
    public int ZNeg;
    public int BaseZIndex;
    public int ConnectorZIndex;
    public int TopTileZIndex;
    private List<Node2D> Nodes = new();
    private bool Selected;

    public int HeightZIndex => BaseZIndex + TerrainInfo.TileHeight;
    
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
        Position = new Vector2(X, Z);
        YSortEnabled = true;
        NodeParent.YSortEnabled = true;
        PackedScene PS = null;

        for (int i = 0; i <= TerrainInfo.TileHeight; i++)
        {
            var y = i == 0
                ? 0
                : OrgHeightOffset + (i - 1) * HeightOffset;

            PS = TileMap[TerrainInfo, y, i].TileNode;
            Nodes.Add(PS.Instantiate<Node2D>());
            NodeParent.AddChild(Nodes[i]);
            Nodes[i].Owner = GetTree().EditedSceneRoot;
            Nodes[i].Position -= new Vector2(0, y);

            Nodes[i].ZIndex = BaseZIndex + i;
            Nodes[i].RotationDegrees = TileUtil.GetTileRotation(TerrainInfo, TileUtil.GetState(TerrainInfo, y, i));
        }

        if (TerrainInfo.TileTopType != TileUtil.TileTopType.None && TerrainInfo.TileType == TileUtil.TileType.Grass)
        {
            var TPS = TopTileMap[TerrainInfo.TopTileState].TileNode;
            var topNode = TPS.Instantiate<Node2D>();
            AddChild(topNode);
            topNode.Position -= new Vector2(0, heightOffset);
            topNode.ZIndex = TopTileZIndex;
            topNode.Owner = GetTree().EditedSceneRoot;
        }
    }

    public Tile Select(Func<Tile, bool> func)
    {
        if (!func(this))
        {
            return null;
        }
        if (Selected) return this;
        
        foreach (Node2D node in Nodes)
        {
            var mesh = node.GetChild<MeshInstance3D>(0);
            mesh.SetSurfaceOverrideMaterial(0, SelectMaterial);
        }
        Selected = true;
        return this;
    }

    public void Deselect()
    {
        foreach (Node2D node in Nodes)
        {
            var mesh = node.GetChild<MeshInstance3D>(0);
            mesh.SetSurfaceOverrideMaterial(0, ActualMaterial);
        }
        Selected = false;
    }
}
