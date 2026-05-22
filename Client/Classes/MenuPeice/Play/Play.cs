using Godot;
using System;

public partial class Play : MenuPeice
{
    [Export] private Button Unranked, Ranked;
    public override void _Ready()
    {
        Unranked.ButtonUp += mainLoggedInMenu.QueueUnranked;
        Ranked.ButtonUp += mainLoggedInMenu.QueueRanked;
    }
}
