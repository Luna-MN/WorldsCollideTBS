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
        TerrainGen1.seed = (int)Globals.GM.gameData.Seed1;
        TerrainGen2.seed = (int)Globals.GM.gameData.Seed2;
        MainGameTerrainGen.seed = (int)Globals.GM.gameData.GameSeed;
        TerrainGen1._Ready();
        TerrainGen2._Ready();
        MainGameTerrainGen._Ready();
        Globals.GM.gameData.TerrainGen = MainGameTerrainGen;
        Globals.GM.gameData.TerrainGen1 = TerrainGen1;
        Globals.GM.gameData.TerrainGen2 = TerrainGen2;
        Globals.GM.gameData.UpdateTileNeighbours();
        UnitTesting();
        test();
    }

    public void test()
    {
        GD.Print(MainGameTerrainGen.worldInfo.TerrainInfo[new Vector2I(0, 0)].Position);
        GD.Print(MainGameTerrainGen.worldInfo.TerrainInfo[new Vector2I(1, 0)].Position);
        GD.Print(MainGameTerrainGen.worldInfo.TerrainInfo[new Vector2I(2, 0)].Position);
        GD.Print(MainGameTerrainGen.worldInfo.TerrainInfo[new Vector2I(3, 0)].Position);
        GD.Print(MainGameTerrainGen.worldInfo.TerrainInfo[new Vector2I(4, 0)].Position);
        GD.Print(MainGameTerrainGen.worldInfo.TerrainInfo[new Vector2I(5, 0)].Position);
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
