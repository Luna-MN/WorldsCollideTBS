using Godot;
using System;

public partial class UnitPanel : Panel
{
    [Export] private RichTextLabel NamePlate;

    public void ChangeUnit(UnitData unit)
    {
        if (unit == null)
        {
            Visible = false;
            return;
        }
        Visible = true;
        NamePlate.Text = unit.UnitName;
    }
}
