using System.Collections.Generic;
using Godot;
using Packets;
using Packets.Util;

public class DefaultMovement : IMovement
{
    public IUnit Unit { get; set; }
    public Node2D Node { get; set; }
    public HexPositionsMessage PrevMove { get; set; }
    public void InitMovement(IUnit unit, Node2D node)
    {
        Unit = unit;
        Node = node;
    }
    public async void Move(List<TerrainInfo> path, bool message = false)
    {
        if (!message)
        {
            SendModePacket(path);
        }

        foreach (var tile in path)
        {
            if (path.IndexOf(tile) == 0)
            {
                continue;
            }
            //move to it, wait till move
            var tween = Node.CreateTween();
            Vector2 midpoint = Node.Position.Lerp(tile.Position, 0.5f);
            tween.TweenProperty(Node, "position", new Vector2(midpoint.X, midpoint.Y), 0.2f).SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.Out);
            await tween.ToSignal(tween, Tween.SignalName.Finished);
            var tween2 = Node.CreateTween();
            tween2.TweenProperty(Node, "position", new Vector2(tile.Position.X, tile.Position.Y), 0.2f).SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In);
            await tween2.ToSignal(tween2, Tween.SignalName.Finished);
        }

        path[0].Unit = null;
        path[^1].Unit = Unit;
    }

    public void SendModePacket(List<TerrainInfo> path)
    {
        var hexPositions = new List<HexPositionMessage>();
        foreach (var tile in path)
        {
            hexPositions.Add(PacketUtil.NewHexPositionMessage(tile.PositionI));
        }

        var move = PacketUtil.NewHexPositions(Unit.Data.UnitId, hexPositions);
        PrevMove = move.HexPositions;
        TrafficManager.Send(move);
    }

    public bool ValidateMovement(HexPositionsMessage hexPositions)
    {
        return !hexPositions.Equals(PrevMove);
    }
}