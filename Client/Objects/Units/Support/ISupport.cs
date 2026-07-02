using Godot;

public interface ISupport
{
    public IUnit Unit { get; set; }
    public Node3D Node { get; set; }
    public void InitSupport(IUnit unit, Node3D node);
    public void Support(IUnit unit);
    public void SendSupportPacket(IUnit unit);
}