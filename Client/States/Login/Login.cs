using Godot;
using System;
using System.Linq;
using Packets;
using Steamworks;

public partial class Login : Control, IState
{
    [Export] public Log log { get; set; }
    public bool IsSmoothState => false;
    public Node[] TransitionNodes { get; set; }
    private GameDataUpdater gameDataUpdater;
    public override void _Ready()
    {
        base._Ready();
        
        Globals.GM.Subscribe(OnPacketReceived, OnWSConnectionClosed);
        SendVersion();
        GetSteamAuth();
    }

    private void SendVersion()
    {
        gameDataUpdater = new GameDataUpdater();
        TrafficManager.Send(Packets.Util.PacketUtil.NewGameVersionPacket(gameDataUpdater.GetVersion()));
    }

    private void GetSteamAuth()
    {
        CSteamID steamId = SteamUser.GetSteamID();

        var identity = new SteamNetworkingIdentity();
        identity.SetSteamID(steamId); 

        var ticket = Globals.SM.GetSteamAuthTicket(identity);
        var packet = new Packet()
        {
            SteamTicket = new SteamAuthTicketMessage()
            {
                SteamUsername = Globals.SM.ClientName,
                Ticket = Google.Protobuf.ByteString.CopyFrom(ticket),
            }
        };
        TrafficManager.Send(packet);
    }
    public void OnPacketReceived(Packet packet)
    {
        log.info($"Received packet from {packet.SenderId}: {packet.MsgCase}");
        if (packet.SenderId != Globals.GM.clientId)
        {
            log.error($"Received packet from wrong client. Expected {Globals.GM.clientId} got {packet.SenderId}");
            return;
        }

        switch (packet.MsgCase)
        {
            case Packet.MsgOneofCase.OK:
                HandleOKMessage();
                break;
            case Packet.MsgOneofCase.Deny:
                HandleDenyMessage(packet.Deny);
                break;
            case Packet.MsgOneofCase.GameData:
                HandleGameData(packet.GameData);
                break;
            case Packet.MsgOneofCase.GameVersion:
                HandleGameVersion();
                break;
            default:
                GD.Print($"Unknown packet received: {packet.MsgCase}");
                break;
        }
    }

    private void HandleGameVersion()
    {
        gameDataUpdater.DeserializeGameData();
    }

    private void HandleOKMessage()
    {
        log.success("Login successful.");
        Globals.GM.SetState(GameManager.state.MainLoggedInMenu);
    }
    private void HandleDenyMessage(DenyResponseMessage msg)
    {
        log.error($"Login failed. {msg.Reason}");
    }
    private void HandleGameData(GameDataMessage msg)
    {
        gameDataUpdater.UpdateVersion(msg.Version.Version, msg);
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
