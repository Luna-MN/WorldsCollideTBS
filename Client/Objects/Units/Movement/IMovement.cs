using System.Collections.Generic;
using Godot;

public interface IMovement
{
    public IUnit Unit { get; set; }
    public Node3D Node { get; set; }
    public void InitMovement(IUnit unit, Node3D node);
    public void Move(List<TerrainInfo> path);
    public void SendModePacket(List<TerrainInfo> path);
}