using Godot;
using System;
using Packets;

public partial class Lobby : Control, IState
{
    [Export]
    public Log log { get; set; }
    public void OnPacketReceived(Packet packet)
    {
        throw new NotImplementedException();
    }

    public void OnWSConnectionClosed()
    {
        throw new NotImplementedException();
    }
}
