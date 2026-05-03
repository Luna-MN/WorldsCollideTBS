using Godot;
using System;
[Tool]
[GlobalClass]
public partial class FeatureArgs : Resource
{
    [ExportGroup("Forest")]
    [Export] public FastNoiseLite ForestNoise;
    [Export] public float Threshold;
}
