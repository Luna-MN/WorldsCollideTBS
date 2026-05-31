using Godot;
using System;
using Packets;

public partial class MainMenu : Control, IState
{
    [Export] private Button Login, Quit;
    public Log log { get; set; }
    public bool IsSmoothState => false;
    public Node[] TransitionNodes { get; set; }
    public void OnPacketReceived(Packet packet)
    {
    }

    public void OnWSConnectionClosed()
    {
    }

    public override void _Ready()
    {
        base._Ready();
        Login.ButtonUp += _on_Login_pressed;
        Quit.ButtonUp += _on_Quit_pressed;
    }

    private void _on_Login_pressed()
    {
        Globals.GM.SetState(GameManager.state.Connect);
    }

    private void _on_Quit_pressed()
    {
        GetTree().Quit();
    }
}
