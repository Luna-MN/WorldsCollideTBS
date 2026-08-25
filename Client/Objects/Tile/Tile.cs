using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class Tile : Node3D
{
    
    [Export] 
    private Node3D NodeParent;
    [Export]
    public StaticBody3D StaticBody;
    [Export]
    private Material SelectMaterial;
    private Material ActualMaterial;
    private TileMap TileMap;
    private TopTileMap TopTileMap;
    public TerrainInfo TerrainInfo;
    private float X;
    private float Z;
    private List<Node3D> Nodes = new();
    private bool Selected;
    
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
            
            Nodes.Add(PS.Instantiate<Node3D>());
            ActualMaterial = Nodes[i].GetChild<MeshInstance3D>(0).GetSurfaceOverrideMaterial(0);
            NodeParent.AddChild(Nodes[i]);
            Nodes[i].Owner = GetTree().EditedSceneRoot;
            Nodes[i].Position = new Vector3(0, i, 0);
            Nodes[i].RotationDegrees = new Vector3(0, TileUtil.GetTileRotation(TerrainInfo, TileUtil.GetState(TerrainInfo, i)), 0);
        }
        if (TerrainInfo.TileTopType != TileUtil.TileTopType.None && TerrainInfo.TileType == TileUtil.TileType.Grass)
        {
            var TPS = TopTileMap[TerrainInfo.TopTileState].TileNode;
            var topNode = TPS.Instantiate<Node3D>();
            AddChild(topNode);
            topNode.Owner = GetTree().EditedSceneRoot;
            topNode.Position = new Vector3(0, TerrainInfo.TileHeight, 0);
        }
        StaticBody.Position = new Vector3(0, TerrainInfo.TileHeight, 0);
    }

    public Tile Select(Func<Tile, bool> func)
    {
        if (!func(this))
        {
            return null;
        }
        if (Selected) return this;
        
        foreach (Node3D node in Nodes)
        {
            var mesh = node.GetChild<MeshInstance3D>(0);
            mesh.SetSurfaceOverrideMaterial(0, SelectMaterial);
        }
        Selected = true;
        return this;
    }

    public void Deselect()
    {
        foreach (Node3D node in Nodes)
        {
            var mesh = node.GetChild<MeshInstance3D>(0);
            mesh.SetSurfaceOverrideMaterial(0, ActualMaterial);
        }
        Selected = false;
    }
}
