using Godot;
using System;
using Packets;

public partial class Lobby : Control, IState
{
    [Export]
    public Log log { get; set; }
    public bool IsSmoothState => false;
    public Node[] TransitionNodes { get; set; }
    [Export] private Button Play, Quit, Invite;
    [Export] public RichTextLabel Player1, Player2;
    public override void _Ready()
    {
        Globals.GM.Subscribe(OnPacketReceived, OnWSConnectionClosed);
        Play.ButtonUp += StartGame;
        Quit.ButtonUp += () => Globals.GM.SetState(GameManager.state.MainLoggedInMenu);
        Invite.ButtonUp += InvitePlayer;
        Globals.GM.gameData = new GameData();
    }

    public void OnPacketReceived(Packet packet)
    {
        log.info("Packet received in lobby" + packet.MsgCase);
        switch (packet.MsgCase)
        {
            case Packet.MsgOneofCase.Opponent:
                OpponentReceived(packet.SenderId, packet.Opponent);
                break;
            case Packet.MsgOneofCase.IDs:
                SetPlayerIds(packet.IDs);
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

    private void SetPlayerIds(IDsMessage ids)
    {
        Globals.GM.gameData.ID1 = ids.IDs[0].Id;
        Globals.GM.gameData.ID2 = ids.IDs[1].Id;
    }
    private void StartGame()
    {
        Globals.GM.SetState(GameManager.state.StartGame);
    }
    public void OnWSConnectionClosed()
    {
        Globals.GM.SetState(GameManager.state.MainMenu);
    }

    private void InvitePlayer()
    {
        throw  new NotImplementedException();
    }
    public override void _ExitTree()
    {
        Globals.GM.Unsubscribe(OnPacketReceived, OnWSConnectionClosed);
    }   
}
