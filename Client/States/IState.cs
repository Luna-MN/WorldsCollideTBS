using Godot;
using System;
using Packets;

public interface IState
{
    public Log log { get; set; }
    public bool IsSmoothState { get; }
    public Node[] TransitionNodes { get; set; }
    void OnPacketReceived(Packet packet);
    void OnWSConnectionClosed();
    public void _Ready();
    public void _ExitTree();
}

public interface ISmoothState : IState
{
    public Node[] PrevObjects { get; set; }
}