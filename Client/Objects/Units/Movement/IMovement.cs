using System.Collections.Generic;
using Godot;

public interface IMovement
{
    public IUnit Unit { get; set; }
    public Node3D Node { get; set; }
    public void InitMovement(IUnit unit, Node3D node);
    public void Move(List<TerrainInfo> path, bool message = false);
    public void SendModePacket(List<TerrainInfo> path);
    public bool ValidateMovement(Packets.HexPositionsMessage hexPositions);
}