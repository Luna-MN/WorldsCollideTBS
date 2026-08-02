using Godot;
using System;

public partial class Ui : CanvasLayer
{
    [Export] public UnitPanel unitPanel;
    [Export] public Button EndTurn;
    [Export] public RichTextLabel Turn;
    public void ChangeTurn()
    {
        if (Globals.GM.CurrentGameData.TurnOrder[Globals.GM.CurrentGameData.currTurnPointer] == Globals.GM.clientId)
        {
            Turn.Text = "Your Turn";
        }
        else
        {
            Turn.Text = "Enemy Turn";
        }
    }
}
