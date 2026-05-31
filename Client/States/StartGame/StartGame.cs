using Godot;
using System;
using Packets;
using Packets.Util;

public partial class StartGame : Node3D, IState
{
    [Export]
    public Log log { get; set; }
    public bool IsSmoothState => false;
    public Node[] TransitionNodes { get; set; }
    public int selectedSeed;
    [Export]
    private TerrainGen terrainGen1, terrainGen2, terrainGen3;
    [Export] private InputHandler inputHandler;
    [Export] private Button confirm;
    public override void _Ready()
    {
        Globals.GM.Subscribe(OnPacketReceived, OnWSConnectionClosed);
        confirm.Visible = false;
        confirm.ButtonUp += confirmClicked;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_click"))
        {
            if (GetTree().Root.GuiGetFocusOwner() != null)
            {
                return;
            }
            if (inputHandler.CurrentMouseNode == null)
            {
                ResetFocus();
                selectedSeed = 0;
                confirm.Visible = false;
                return;
            }
            var node = inputHandler.CurrentMouseNode.GetParent<TerrainGen>();
            selectedSeed = node.seed;
            confirm.Visible = true;
            GrabFocus();
        }
    }

    public void OnPacketReceived(Packet packet)
    {
        switch (packet.MsgCase)
        {
            case Packet.MsgOneofCase.Seed:
                HandleSeedMessage(packet.Seed);
                break;
            case Packet.MsgOneofCase.StartGame:
                HandleStartGameMessage(packet.StartGame);
                break;
        }
    }
    private void HandleSeedMessage(SeedMessage msg)
    {
        if (msg.Seed.Count < 3)
        {
            log.error("Seed is too short.");
            return;
        }
        terrainGen1.seed = (int)msg.Seed[0];
        terrainGen2.seed = (int)msg.Seed[1];
        terrainGen3.seed = (int)msg.Seed[2];
        terrainGen1._Ready();
        terrainGen2._Ready();
        terrainGen3._Ready();
    }
    private void confirmClicked()
    {
        if (selectedSeed == 0) return;
        
        if (Globals.GM.gameData.ID1 == Globals.GM.clientId)
        {
            Globals.GM.gameData.Seed1 = selectedSeed;
        }
        else
        {
            Globals.GM.gameData.Seed2 = selectedSeed;
        }
        
        TrafficManager.Send(PacketUtil.NewSeedPacket(selectedSeed));
    }

    private void GrabFocus()
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

    private void ResetFocus()
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
        Globals.GM.SetState(GameManager.state.AwaitingGameData);
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
