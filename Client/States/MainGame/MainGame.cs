using Godot;
using System;
using Packets;

public partial class MainGame : Node3D, ISmoothState
{
    [Export]
    public Log log { get; set; }
    public Node[] PrevObjects { get; set; }
    [Export] public PackedScene unitScene;
    public bool IsSmoothState => true;
    public Node[] TransitionNodes { get; set; }
    [Export] private TerrainGen TerrainGen1, MainGameTerrainGen, TerrainGen2;

    public override void _Ready()
    {
        Globals.GM.Subscribe(OnPacketReceived, OnWSConnectionClosed);
        
        TerrainGen1.seed = (int)Globals.GM.CurrentGameData.LeftSeed;
        TerrainGen2.seed = (int)Globals.GM.CurrentGameData.RightSeed;
        MainGameTerrainGen.seed = (int)Globals.GM.CurrentGameData.GameSeed;
        TerrainGen1._Ready();
        TerrainGen2._Ready();
        MainGameTerrainGen._Ready();
        CallDeferred(nameof(SceneInitializeDeferred));
    }

    private void SceneInitializeDeferred()
    {
        var n = (PrevObjects[0] as Node3D);
        if (Globals.GM.CurrentGameData.MySide == CurrentGameData.Side.left)
        {
            TerrainGen1.QueueFree();
            TerrainGen1 = null;
            TerrainGen1 = n.GetNode<TerrainGen>("TerrainGen");
            n.Position = new Vector3(-13.8f, 0, 0);
        }
        else
        {
            TerrainGen2.QueueFree();
            TerrainGen2 = null;
            TerrainGen2 = n.GetNode<TerrainGen>("TerrainGen");
            n.Position = new Vector3(13.8f, 0, 0);
        }
        
        Globals.GM.CurrentGameData.TerrainGen = MainGameTerrainGen;
        Globals.GM.CurrentGameData.TerrainGen1 = TerrainGen1;
        Globals.GM.CurrentGameData.TerrainGen2 = TerrainGen2;
        Globals.GM.CurrentGameData.UpdateTileNeighbours();
        var myT = n.GetNode<TerrainGen>("TerrainGen");
        n.RemoveChild(myT);
        foreach (var unit in Globals.GM.CurrentGameData.MyUnits.Values)
        {
            n.RemoveChild(unit as Node3D);
        }
        CallDeferred(nameof(AddBackDeferred), myT);
        InitEnemyUnits();
    }

    private void InitEnemyUnits()
    {
        foreach (var unit in Globals.GM.CurrentGameData.EnemyUnits.Values)
        {
            var nodeU = (unit as Node3D);
            AddChild(nodeU);
            var uTile = Globals.GM.CurrentGameData.Tiles[unit.PositionI];
            nodeU.Position = uTile.Position + new Vector3(0, uTile.TileHeight, 0);
        }
    }

    private void AddBackDeferred(TerrainGen myT)
    {
        AddChild(myT);
        if (Globals.GM.CurrentGameData.MySide == CurrentGameData.Side.left)
        {
            myT.Position = new Vector3(-13.8f, 0, 0);
        }
        else
        {
            myT.Position = new Vector3(13.8f, 0, 0);
        }

        foreach (var unit in Globals.GM.CurrentGameData.MyUnits.Values)
        {
            var nodeU = (unit as Node3D);
            AddChild(nodeU);
        }

        foreach (var tile in myT.worldInfo.TerrainInfo.Values)
        {
            if (tile.Unit != null)
            {
                tile.Unit.Position = tile.Unit.TileNode.GlobalPosition + new Vector3(0, tile.TileHeight, 0);
                        
            }

        }
    }
    public void OnPacketReceived(Packet packet)
    {
        
    }

    public void OnWSConnectionClosed()
    {
        Globals.GM.SetState(GameManager.state.MainMenu);
    }
    public override void _ExitTree()
    {
        Globals.GM.Unsubscribe(OnPacketReceived, OnWSConnectionClosed);
    }
    

}
