using Godot;
using System;
using Packets;

public interface IState
{
    public Log log { get; set; }
    void OnPacketReceived(Packet packet);
    void OnWSConnectionClosed();
}