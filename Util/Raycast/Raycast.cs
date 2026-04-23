using Godot;
using Godot.Collections;

namespace Util;

public static class RayCast
{
    public static Viewport port;
    public static SceneTree tree;
    public static World3D world;

    public static void Set(SceneTree tree, Viewport viewport, World3D world)
    {
        RayCast.tree = tree;
        RayCast.port = viewport;
        RayCast.world = world;
    }

    public static Vector3 CastPosition()
    {
        Dictionary result = Cast();
        if (result == null) return Vector3.Zero;
        if (result.TryGetValue("position", out var positionVariant))
            return (Vector3)positionVariant;
        return Vector3.Zero;
    }

    public static Dictionary Cast()
    {
        if (world == null || port == null || tree == null)
        {
            GD.Print(port, world, tree);
            return null;
        }

        Camera3D cam = tree.Root.GetCamera3D();
        if (cam == null)
        {
            // No active camera yet (or current camera is not in the root viewport).
            return null;
        }

        PhysicsDirectSpaceState3D spaceState = world.DirectSpaceState;
        Vector2 mousePos = port.GetMousePosition();

        var rayO = cam.ProjectRayOrigin(mousePos);
        var rayE = rayO + cam.ProjectRayNormal(mousePos) * 2000f;

        var query = PhysicsRayQueryParameters3D.Create(rayO, rayE);
        query.CollideWithAreas = true;
        query.CollideWithBodies = true;

        Dictionary result = spaceState.IntersectRay(query);
        
        return result;
    }
    public static Node3D CastObject()
    {
        Dictionary result = Cast();
        if (result == null) return null;
        if (result.TryGetValue("collider", out var colliderVariant))
            return (Node3D)colliderVariant;
        return null;
    }
}