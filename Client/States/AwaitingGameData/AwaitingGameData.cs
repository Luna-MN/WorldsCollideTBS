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
        if (Globals.GM.gameData.ID1 == Globals.GM.clientId)
        {
            Globals.GM.gameData.Seed2 = seedMessage.Seed[0];
        }
        else
        {
            Globals.GM.gameData.Seed1 = seedMessage.Seed[0];
        }
        
        log.info($"Seeds: {Globals.GM.gameData.Seed1} {Globals.GM.gameData.Seed2}");
        Globals.GM.gameData.GameSeed = seedMessage.Seed[1];
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
