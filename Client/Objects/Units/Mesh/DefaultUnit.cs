using Godot;
using System;

public partial class DefaultUnit : Node3D
{
    [Export] public RichTextLabel NamePlate;
    [Export] public ProgressBar HPBar;
    [Export] public MeshInstance3D Mesh;

    public void ChangeColor(Color color)
    {
        GD.Print($"{Name} ChangeColor called. Mesh assigned: {Mesh != null}, Mesh resource assigned: {Mesh?.Mesh != null}, Color: {color}");

        if (Mesh?.Mesh == null)
        {
            GD.PrintErr($"{Name} cannot change color because Mesh or Mesh.Mesh is null.");
            return;
        }

        Material existingMaterial =
            Mesh.GetSurfaceOverrideMaterial(0) ??
            Mesh.Mesh.SurfaceGetMaterial(0);

        StandardMaterial3D material;

        if (existingMaterial is StandardMaterial3D standardMaterial)
        {
            material = standardMaterial.Duplicate() as StandardMaterial3D;
        }
        else
        {
            material = new StandardMaterial3D();
        }

        material.AlbedoColor = color;
        Mesh.SetSurfaceOverrideMaterial(0, material);

        GD.Print($"{Name} override after set: {Mesh.GetSurfaceOverrideMaterial(0) != null}");
    }

}
