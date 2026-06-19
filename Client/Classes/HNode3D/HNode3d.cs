using Godot;
using System.Collections.Generic;

[Tool]
[GlobalClass]
public partial class HNode3d : Node3D
{
    [ExportToolButton("Generate Children")]
    private Callable GenerateChildrenCallable => Callable.From(GenerateChildren);

    public enum Direction
    {
        X,
        Y,
        Z
    }

    [Export] public Direction Dir;
    [Export] public float FullSize;

    private readonly List<Node3D> Children = [];
    private Vector3 D;

    public override void _EnterTree()
    {
        ChildEnteredTree += OnChildEnteredTree;

        RebuildChildrenList();
        GenerateChildren();
    }

    public override void _ExitTree()
    {
        ChildEnteredTree -= OnChildEnteredTree;
    }

    private void OnChildEnteredTree(Node child)
    {
        if (child is not Node3D node3D)
            return;

        if (!Children.Contains(node3D))
            Children.Add(node3D);

        GenerateChildren();
    }

    private void RebuildChildrenList()
    {
        Children.Clear();

        foreach (Node child in GetChildren())
        {
            if (child is Node3D node3D)
                Children.Add(node3D);
        }
    }

    public void GenerateChildren()
    {
        D = Dir switch
        {
            Direction.X => new Vector3(1, 0, 0),
            Direction.Y => new Vector3(0, 1, 0),
            Direction.Z => new Vector3(0, 0, 1),
            _ => Vector3.Zero
        };

        int childCount = Children.Count;

        for (int i = 0; i < childCount; i++)
        {
            Node3D child = Children[i];

            if (!IsInstanceValid(child))
                continue;

            if (childCount == 1)
            {
                child.Position = Vector3.Zero;
            }
            else
            {
                child.Position = D * ((FullSize * i / (childCount - 1)) - (FullSize / 2));
            }
        }
    }
}