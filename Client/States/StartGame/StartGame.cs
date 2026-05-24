using Godot;
using System;
using Packets;

public partial class StartGame : Node3D, IState
{
    [Export]
    public Log log { get; set; }

    public override void _Ready()
    {
        TrafficManager.packetRecived += OnPacketReceived;
        Globals.WS.connectionClosed += OnWSConnectionClosed;
    }

    public void OnPacketReceived(Packet packet)
    {
        throw new NotImplementedException();
    }

    public void OnWSConnectionClosed()
    {
        throw new NotImplementedException();
    }
}
