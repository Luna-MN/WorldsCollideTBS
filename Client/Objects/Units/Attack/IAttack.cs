using Godot;

public interface IAttack : ISkill
{
    public AttackData Data { get; set; }
}