using Godot;
using System;

public partial class Asteroid : Node3D
{
    [Export]
    private CustomTerrainInfo TerrainInfo;
    [Export]
    private float speed;
    [Export]
    private Area3D Area;
    public int DestroyTimerTime;
    private Timer DestroyTimer;
    private Vector3 StartPos;
    private Vector3 StopPos;
    public override void _Ready()
    {
        DestroyTimer = new Timer()
        {
            Autostart = true,
            OneShot = true,
            WaitTime = DestroyTimerTime,
        };
        DestroyTimer.Timeout += async () =>
        {
            var tw = CreateTween();
            tw.TweenProperty(this, "scale", new Vector3(0.001f, 0.001f, 0.001f), 1f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
            await tw.ToSignal(tw, Tween.SignalName.Finished);
            QueueFree();
        };
        AddChild(DestroyTimer);
        var rng = new RandomNumberGenerator();
        var dir = rng.RandiRange(0, 3);
        Position = dir switch
        {
            0 => new Vector3(rng.RandfRange(-30, 30), 0, rng.RandfRange(-30, -25)),
            1 => new Vector3(rng.RandfRange(25, 30), 0, rng.RandfRange(-30, 30)),
            2 => new Vector3(rng.RandfRange(-30, 30), 0, rng.RandfRange(25, 30)),
            3 => new Vector3(rng.RandfRange(-30, -25), 0, rng.RandfRange(-30, 30)),
            _ => Position
        };
        StartPos = Position;
        StopPos = new Vector3(rng.RandfRange(-30, 30), 0, rng.RandfRange(-30, 30));
    }
    

    public override void _Process(double delta)
    {
        Position = Position.Lerp(StopPos, (float)(delta * speed));
    }
    
}
