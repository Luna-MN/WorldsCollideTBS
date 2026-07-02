using Godot;
using System;

public partial class Ui : CanvasLayer
{
    [Export] public UnitPanel unitPanel;
    [Export] public Button EndTurn;
    [Export] public RichTextLabel Turn;
    public void ChangeTurn()
    {
        if (Turn.Text == "Your Turn")
        {
            Turn.Text = "Enemy Turn";
        }
        else
        {
            Turn.Text = "Your Turn";
        }
    }
}
