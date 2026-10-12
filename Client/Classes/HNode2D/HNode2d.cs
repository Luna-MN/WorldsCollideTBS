using Godot;
using System.Collections.Generic;

[Tool]
[GlobalClass]
public partial class HNode2d : Node2D
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

    private readonly List<Node2D> Children = [];
    private Vector2 D;

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
        if (child is not Node2D node2D)
            return;

        if (!Children.Contains(node2D))
            Children.Add(node2D);

        GenerateChildren();
    }

    private void RebuildChildrenList()
    {
        Children.Clear();

        foreach (Node child in GetChildren())
        {
            if (child is Node2D node2D)
                Children.Add(node2D);
        }
    }

    public void GenerateChildren()
    {
        D = Dir switch
        {
            Direction.X => new Vector2(1, 0),
            Direction.Y => new Vector2(0, 1),
            _ => Vector2.Zero
        };

        int childCount = Children.Count;

        for (int i = 0; i < childCount; i++)
        {
            Node2D child = Children[i];

            if (!IsInstanceValid(child))
                continue;

            if (childCount == 1)
            {
                child.Position = Vector2.Zero;
            }
            else
            {
                child.Position = D * ((FullSize * i / (childCount - 1)) - (FullSize / 2));
            }
        }
    }
}