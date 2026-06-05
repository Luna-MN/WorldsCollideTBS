using Godot;

public interface IAttack
{
    public IUnit Unit { get; set; }
    public Node3D Node { get; set; }
    public void InitAttack(IUnit unit, Node3D node);
    public void Attack(IUnit unit);
    public void SendAttackPacket(IUnit unit);
}