using Godot;
using System;
using Packets;

public partial class MainGame : Node3D, IState
{
    [Export]
    public Log log { get; set; }
    public bool IsSmoothState => false;
    public Node[] TransitionNodes { get; set; }
    [Export] private TerrainGen TerrainGen1, MainGameTerrainGen, TerrainGen2;
    public override void _Ready()
    {
        Globals.GM.Subscribe(OnPacketReceived, OnWSConnectionClosed);
        TerrainGen1.seed = (int)Globals.GM.gameData.Seed1;
        TerrainGen2.seed = (int)Globals.GM.gameData.Seed2;
        MainGameTerrainGen.seed = (int)Globals.GM.gameData.GameSeed;
        TerrainGen1._Ready();
        TerrainGen2._Ready();
        MainGameTerrainGen._Ready();
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
