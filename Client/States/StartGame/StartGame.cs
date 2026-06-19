using Godot;
using System;
using System.Collections.Generic;
using Packets;
using Packets.Util;

public partial class StartGame : Node3D, IState
{
    [Export]
    public Log log { get; set; }
    [Export]
    private PackedScene unitScene;
    [Export]
    private CustomTerrainInfo tile;
    public bool IsSmoothState => false;
    public Node[] TransitionNodes { get; set; }
    public int selectedSeed;
    [Export]
    private TerrainGen terrainGen1, terrainGen2, terrainGen3;
    [Export] private InputHandler inputHandler;
    [Export] public Button confirm;
    public bool seedSelected;
    public override void _Ready()
    {
        Globals.GM.Subscribe(OnPacketReceived, OnWSConnectionClosed);
        confirm.Visible = false;
        confirm.ButtonUp += confirmClicked;
        test();   
    }

    private void test()
    {
        var u = unitScene.Instantiate<DefaultUnit>();
        AddChild(u);
        u.Position = new Vector3(tile.GlobalPosition.X, tile.TerrainInfo.TileHeight, tile.GlobalPosition.Z);
        tile.TerrainInfo.Unit = u;
    }

    public override void _Process(double delta)
    {

    }

    public void OnPacketReceived(Packet packet)
    {
        switch (packet.MsgCase)
        {
            case Packet.MsgOneofCase.Seed:
                switch (packet.Seed.Seed.Count)
                {
                    case 3:
                        ThreeSeedMessageReceived(packet.Seed);
                        break;
                    case 2:
                        TwoSeedMessageReceived(packet.Seed);
                        break;
                    case 1:
                        log.error("Seed is too short.");
                        break;
                }
                break;
            case Packet.MsgOneofCase.StartGame:
                HandleStartGameMessage(packet.StartGame);
                break;
        }
    }
    private void ThreeSeedMessageReceived(SeedMessage msg)
    {
        if (msg.Seed.Count < 3)
        {
            log.error("Seed is too short.");
            return;
        }
        terrainGen1.seed = msg.Seed[0];
        terrainGen2.seed = msg.Seed[1];
        terrainGen3.seed = msg.Seed[2];
        terrainGen1._Ready();
        terrainGen2._Ready();
        terrainGen3._Ready();
    }
    private void TwoSeedMessageReceived(SeedMessage seedMessage)
    {
        log.info("Seed received.");
        if (Globals.GM.CurrentGameData.MySide == CurrentGameData.Side.left)
        {
            Globals.GM.CurrentGameData.RightSeed = seedMessage.Seed[0];
        }
        else
        {
            Globals.GM.CurrentGameData.LeftSeed = seedMessage.Seed[0];
        }
        
        log.info($"Seeds: {Globals.GM.CurrentGameData.LeftSeed} {Globals.GM.CurrentGameData.RightSeed}");
        Globals.GM.CurrentGameData.GameSeed = seedMessage.Seed[1];
    }
    private void confirmClicked()
    {
        if (selectedSeed == 0) return;
        
        if (Globals.GM.CurrentGameData.MySide == CurrentGameData.Side.left)
        {
            Globals.GM.CurrentGameData.LeftSeed = selectedSeed;
        }
        else
        {
            Globals.GM.CurrentGameData.RightSeed = selectedSeed;
        }
        seedSelected = true;
        TrafficManager.Send(PacketUtil.NewSeedPacket(selectedSeed));
        HideSeeds();
    }
    

    
    private async void HideSeeds()
    {
        TerrainGen[] gens = [terrainGen1, terrainGen2, terrainGen3];
        Tween tween = null;
        TerrainGen selectedGen = null;
        List<TerrainGen> toDestroy = [];
        foreach (var gen in gens)
        {
            var tw = gen.CreateTween();
            if (gen.seed == selectedSeed)
            {
                tw.TweenProperty(gen, "scale", new Vector3(1.25f, 1.25f, 1.25f), 0.2f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
                tween = tw;
                selectedGen = gen;
            }
            else
            {
                tw.TweenProperty(gen, "scale", new Vector3(0.001f, 0.001f, 0.001f), 0.2f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
                toDestroy.Add(gen);
            }
        }
        await tween.ToSignal(tween, Tween.SignalName.Finished);
        foreach (var gen in toDestroy)
        {
            gen.QueueFree();
        }
        tween = CreateTween();
        tween.TweenProperty(selectedGen, "position", new Vector3(0, 0, 0), 0.2f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
    }
    public void GrabFocus()
    {
        TerrainGen[] gens = [terrainGen1, terrainGen2, terrainGen3];
        foreach (var gen in gens)
        {
            var tw = gen.CreateTween();
            if (gen.seed == selectedSeed)
            {
                tw.TweenProperty(gen, "scale", new Vector3(1.25f, 1.25f, 1.25f), 0.2f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
            }
            else
            {
                tw.TweenProperty(gen, "scale", new Vector3(0.75f, 0.75f, 0.75f), 0.2f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
            }
        }
    }

    public void ResetFocus()
    {
        TerrainGen[] gens = [terrainGen1, terrainGen2, terrainGen3];
        foreach (var gen in gens)
        {
            var tw = gen.CreateTween();
            tw.TweenProperty(gen, "scale", new Vector3(1f, 1f, 1f), 0.2f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
        }
    }
    private void HandleStartGameMessage(StartGameMessage msg)
    {
        TransitionNodes = [terrainGen1, terrainGen2, terrainGen3];
    }
    public void OnWSConnectionClosed()
    {
        throw new NotImplementedException();
    }
    public override void _ExitTree()
    {
        Globals.GM.Unsubscribe(OnPacketReceived, OnWSConnectionClosed);
    }
}
