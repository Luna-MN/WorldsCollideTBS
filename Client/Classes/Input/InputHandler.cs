using Godot;
using System;
using Util;

public partial class InputHandler : Node3D
{
    private Vector3 MousePosition;
    private Node3D CurrentMouseNode;
    public override void _Ready()
    {
        RayCast.Set(GetTree(), GetViewport(), GetWorld3D());
    }

    public override void _Process(double delta)
    {
        MousePos();
    }

    private void MousePos()
    {
        MousePosition = RayCast.CastPosition();
        var obj = RayCast.CastObject()?.GetParent<Node3D>();
        
        if (obj == null)
        {
            if (CurrentMouseNode != null)
            {
                CurrentMouseNode.Position = new Vector3(CurrentMouseNode.Position.X, 0, CurrentMouseNode.Position.Z);
            }
            return;
        }
        if (obj == CurrentMouseNode) return;
        CurrentMouseNode ??= obj;

        CurrentMouseNode.Position = new Vector3(CurrentMouseNode.Position.X, 0, CurrentMouseNode.Position.Z);
        CurrentMouseNode = obj;
        CurrentMouseNode.Position = new Vector3(CurrentMouseNode.Position.X, 0.25f, CurrentMouseNode.Position.Z);
    }
}
