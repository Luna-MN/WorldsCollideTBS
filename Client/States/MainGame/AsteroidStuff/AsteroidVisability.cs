using Godot;
using System;

public partial class AsteroidVisability : Area3D
{
    public override void _Ready()
    {
        BodyEntered += (async body =>
        {
            var tw = CreateTween();
            tw.TweenProperty(body.GetParent<Node3D>(), "scale", new Vector3(0.001f, 0.001f, 0.001f), 1f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
            await tw.ToSignal(tw, Tween.SignalName.Finished);
            body.Visible = false;
        });
        BodyExited += (body =>
        {
            var tw = CreateTween();
            body.Visible = true;
            tw.TweenProperty(body.GetParent<Node3D>(), "scale", new Vector3(1f, 1f, 1f), 1f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
        });
    }
}
