using Godot;
using System;

public partial class DefaultUnit : Node2D
{
    [Export] public RichTextLabel NamePlate;
    [Export] public ProgressBar HPBar;
    [Export] public MeshInstance2D Mesh;

    public void ChangeColor(Color color)
    {
        GD.Print("Change Color not implemented in 2D");
    }

}
