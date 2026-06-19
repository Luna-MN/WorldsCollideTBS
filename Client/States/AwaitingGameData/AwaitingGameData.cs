using Godot;
using System;
using Packets;

public partial class AwaitingGameData : Node3D, ISmoothState
{
    [Export]
    public Log log { get; set; }
    public bool IsSmoothState => true;
    public Node[] TransitionNodes { get; set; } = new Node[3];
    public Node[] PrevObjects { get; set; }
    public override void _Ready()
    {
        Globals.GM.Subscribe(OnPacketReceived, OnWSConnectionClosed);
    }
    public void OnPacketReceived(Packet packet)
    {
        switch (packet.MsgCase)
        {
            case Packet.MsgOneofCase.Seed:
                SeedMessageReceived(packet.Seed);
                break;
        }
    }

    private void SeedMessageReceived(SeedMessage seedMessage)
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
        Globals.GM.SetState(GameManager.state.Game);
    }
    public void OnWSConnectionClosed()
    {
        Globals.GM.SetState(GameManager.state.MainMenu);
    }
    public void ExitTree()
    {
        Globals.GM.Unsubscribe(OnPacketReceived, OnWSConnectionClosed);
    }
    public override void _ExitTree()
    {
        ExitTree();
    }
}
