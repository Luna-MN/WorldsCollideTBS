using Godot;
using System;
using Packets;

public interface State
{
    void OnPacketReceived(Packet packet);
    void OnWSConnectionClosed();

    void ExitTree()
    {
        Globals.WS.connectionClosed -= OnWSConnectionClosed;
        TrafficManager.packetRecived -= OnPacketReceived;
    }
}