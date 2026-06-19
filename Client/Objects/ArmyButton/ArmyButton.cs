using Godot;
using System;

public partial class ArmyButton : Button
{
    public void SetArmyName(string name)
    {
        Text = Text.Replace("(Army Name)", name);
    }
    public void SetArmyIcon(Texture2D icon)
    {
        Icon = icon;
    }
    public void SetArmyFaction(string faction)
    {
        Text = Text.Replace("(Faction Name)", faction);
    }
    public void SetArmyDescription(string description)
    {
        Text = Text.Replace("(Army Description)", description);
    }
}
