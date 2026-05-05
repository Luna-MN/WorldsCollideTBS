using Godot;
using System;

public partial class Main : Control
{
    public override void _Ready()
    {
        Globals.WS = GetTree().Root.GetNode<Websocket>("Websocket");
        Globals.GM = GetTree().Root.GetNode<GameManager>("GameManager");
        Globals.GM.SetState(GameManager.state.MainMenu);
    }
}
