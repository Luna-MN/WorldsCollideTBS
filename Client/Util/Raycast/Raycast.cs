using Godot;
using Godot.Collections;

namespace Util;

public static class RayCast
{
    public static SceneTree tree;

    public static void Set(SceneTree tree)
    {
        RayCast.tree = tree;
    }

    public static Vector2 CastPosition()
    {
        Dictionary result = Cast();
        if (result == null) return Vector2.Zero;
        if (result.TryGetValue("position", out var positionVariant))
            return (Vector2)positionVariant;
        return Vector2.Zero;
    }

    public static Dictionary Cast()
    {
         var query = new PhysicsPointQueryParameters2D()

        {

        Position = tree.Root.GetViewport().GetMousePosition(),

        CollideWithAreas = true

        };

        return tree.Root.GetWorld2D().DirectSpaceState.IntersectPoint(query)[0]; 
    }
    public static T CastObject<T>() where T : Node2D
    {
        Dictionary result = Cast();
        if (result == null) return null;

        if (result.TryGetValue("collider", out var colliderVariant))
            return colliderVariant.AsGodotObject() as T;

        return null;
    }

}