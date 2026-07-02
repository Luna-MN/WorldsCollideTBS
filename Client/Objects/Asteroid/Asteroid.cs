using Godot;
using System;

public partial class Asteroid : Node3D
{
    [Export]
    private CustomTerrainInfo TerrainInfo;
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
        CallDeferred(nameof(MaskLayerDeferred));
    }

    private void MaskLayerDeferred()
    {
        TerrainInfo.hexInstance.StaticBody.CollisionMask = 4;
        TerrainInfo.hexInstance.StaticBody.CollisionLayer = 4;
    }
}
