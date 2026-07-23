using Godot;
using Packets;


public interface ISkill
{
    public IUnit Unit { get; set; }
    public string Name();
    public Node3D Node { get; set; }
    public void Init(IUnit unit, Node3D node, SkillData data);
    public SkillType Type();
    public void Use(Vector2I position);
    public void SendPacket(Vector2I position, int skillId);
}