using Godot;
using System;
using System.Collections.Generic;

public partial class Asteroids : Node3D
{
    [Export] public int CreationTimerTime;
    [Export] public int AsteroidCount;
    [Export] public PackedScene AsteroidScene;
    private Timer CreationTimer;
    private List<Asteroid> asteroids = [];
    public override void _Ready()
    {
        for (int i = 0; i < AsteroidCount; i++)
        {
            var asteroid = AsteroidScene.Instantiate<Asteroid>();
            asteroid.DestroyTimerTime = CreationTimerTime * i;
            AddChild(asteroid);
            asteroids.Add(asteroid);
        }

        CreationTimer = new Timer()
        {
            Autostart = true,
            OneShot = true,
            WaitTime = CreationTimerTime
        };
        CreationTimer.Timeout += CreateAsteroid;
        AddChild(CreationTimer);
    }

    private void CreateAsteroid()
    {
        var asteroid = AsteroidScene.Instantiate<Asteroid>();
        asteroid.DestroyTimerTime = CreationTimerTime * AsteroidCount;
        AddChild(asteroid);
        asteroids.Add(asteroid);
        CreationTimer.Start();
    }
}
