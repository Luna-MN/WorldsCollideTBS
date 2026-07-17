using Godot;


public interface ISkill
{
    public IUnit Unit { get; set; }
    public string Name();
    public Node3D Node { get; set; }
    public void Init(IUnit unit, Node3D node, SkillData data);
}