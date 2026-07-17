using Godot;

public interface IAttack : ISkill
{
    public AttackData Data { get; set; }
    public void Attack(IUnit unit);
    public void SendAttackPacket(IUnit unit);
}