using Godot;
using System;
using Packets;

public partial class Connect : Node, State
{
    [Export] public Log log { get; set; }
    public bool idSet;
    public override void _Ready()
    {
        Globals.WS.connectedToServer += OnWSConnectedToServer;
        Globals.WS.connectionClosed += OnWSConnectionClosed;
        TrafficManager.packetRecived += OnPacketReceived;
        
        Globals.WS.connectToUrl("wss://localhost:8080/ws", TlsOptions.ClientUnsafe());
    }

    public void OnPacketReceived(Packet packet)
    {
        var senderId = packet.SenderId;
        log.info($"Received packet from {senderId}: {packet.MsgCase}");
        if (packet.MsgCase == Packet.MsgOneofCase.Id)
        {
            HandleIdMessage(senderId, packet.Id);
        }
    }
    public void HandleIdMessage(ulong senderId, IdMessage msg)
    {
        log.info("Handling ID message. " + senderId);
        log.info($"Received ID: {msg.Id}");
        log.info("Setting client ID.");
        Globals.GM.clientId = msg.Id;
        log.info($"Client ID: {Globals.GM.clientId} sender ID: {senderId}");
        idSet = true;
        Globals.GM.SetState(GameManager.state.Login);
    }
    public void OnWSConnectedToServer()
    {
        log.success("Connected to server.");
    }
    public void OnWSConnectionClosed()
    {
        log.warning("Connection closed.");
    }
    
    public void ExitTree()
    {
        Globals.WS.connectedToServer -= OnWSConnectedToServer;
        Globals.WS.connectionClosed -= OnWSConnectionClosed;
        TrafficManager.packetRecived -= OnPacketReceived;
        ((State)this).ExitTree();
    }
}
