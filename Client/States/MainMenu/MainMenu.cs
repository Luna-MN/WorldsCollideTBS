using Godot;
using System;

public partial class MainMenu : Control
{
    [Export] private Button Login, Quit;
    public override void _Ready()
    {
        base._Ready();
        Login.ButtonUp += _on_Login_pressed;
        Quit.ButtonUp += _on_Quit_pressed;
    }

    private void _on_Login_pressed()
    {
        
    }

    private void _on_Quit_pressed()
    {
        GetTree().Quit();
    }
}
