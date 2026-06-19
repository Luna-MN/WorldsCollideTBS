using Godot;
using System;
using Packets;

public partial class MainGame : Node3D, IState
{
    [Export]
    public Log log { get; set; }

    [Export] public PackedScene unitScene;
    public bool IsSmoothState => false;
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
        Globals.GM.CurrentGameData.TerrainGen = MainGameTerrainGen;
        Globals.GM.CurrentGameData.TerrainGen1 = TerrainGen1;
        Globals.GM.CurrentGameData.TerrainGen2 = TerrainGen2;
        Globals.GM.CurrentGameData.UpdateTileNeighbours();
        UnitTesting();
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

    private void UnitTesting()
    {
        var unit = unitScene.Instantiate<DefaultUnit>();
        var pTile = MainGameTerrainGen.worldInfo.TerrainInfo[new Vector2I(0, 0)];
        unit.Position = pTile.Position;
        unit.Position += new Vector3(0, pTile.TileHeight, 0);
        pTile.Unit = unit;
        AddChild(unit);
    }
}
