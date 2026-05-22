using Godot;
using System;
using Packets;

public partial class Lobby : Control, IState
{
    [Export]
    public Log log { get; set; }

    [Export] private Button Play, Quit, Invite;
    [Export] public RichTextLabel Player1, Player2;
    public override void _Ready()
    {
        TrafficManager.packetRecived += OnPacketReceived;
        Globals.WS.connectionClosed += OnWSConnectionClosed;
        Play.ButtonUp += () => Globals.GM.SetState(GameManager.state.Game);
        Quit.ButtonUp += () => Globals.GM.SetState(GameManager.state.MainLoggedInMenu);
        Invite.ButtonUp += InvitePlayer;
    }

    public void OnPacketReceived(Packet packet)
    {
        log.info("Packet received in lobby" + packet.MsgCase);
        switch (packet.MsgCase)
        {
            case Packet.MsgOneofCase.Opponent:
                OpponentReceived(packet.SenderId, packet.Opponent);
                break;
            case Packet.MsgOneofCase.StartGame:
                StartGame();
                break;
        }
    }

    private void OpponentReceived(UInt64 senderId, OpponentMessage msg)
    {
        Globals.GM.opponentId = senderId;
        Globals.GM.opponentUserString = msg.Id;
        Globals.GM.opponentIsSteamClient = msg.Steam;
    }

    private void StartGame()
    {
        Globals.GM.SetState(GameManager.state.Game);
    }
    public void OnWSConnectionClosed()
    {
        Globals.GM.SetState(GameManager.state.MainMenu);
    }

    private void InvitePlayer()
    {
        throw  new NotImplementedException();
    }
}
