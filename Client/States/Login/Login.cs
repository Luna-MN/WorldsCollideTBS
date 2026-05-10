using Godot;
using System;
using System.Linq;
using Packets;
using Steamworks;

public partial class Login : Control, State
{
    [Export] private Log log;
    public override void _Ready()
    {
        base._Ready();
        
        Globals.WS.connectionClosed += OnWSConnectionClosed;
        TrafficManager.packetRecived += OnPacketReceived;
        
        GetSteamAuth();
    }

    private void GetSteamAuth()
    {
        var identity = new SteamNetworkingIdentity();
        var ticket = Globals.SM.GetSteamAuthTicket(identity);
        var strId = identity.GetSteamID().ToString();
        
        var packet = new Packet()
        {
            SteamTicket = new SteamAuthTicketMessage()
            {
                SteamUsername = Globals.SM.ClientName,
                Ticket = Google.Protobuf.ByteString.CopyFrom(ticket),
                Identity = strId
            }
        };
        Globals.WS.Send(packet);
    }
    public void OnPacketReceived(Packet packet)
    {
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
            default:
                GD.Print($"Unknown packet received: {packet.MsgCase}");
                break;
        }
    }

    private void HandleOKMessage()
    {
        log.success("Login successful.");
    }
    private void HandleDenyMessage(DenyResponseMessage msg)
    {
        log.error($"Login failed. {msg.Reason}");
    }
    public void OnWSConnectionClosed()
    {
        Globals.GM.SetState(GameManager.state.MainMenu);
    }
    public void ExitTree()
    {
        Globals.WS.connectionClosed -= OnWSConnectionClosed;
        TrafficManager.packetRecived -= OnPacketReceived;
        ((State)this).ExitTree();
    }
}
