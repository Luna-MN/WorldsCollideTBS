using System.Collections.Generic;
using Godot;

public interface IUnit
{

    public Vector3 Position { get; set; }
    public Vector2I PositionI { get; set; }
    public UnitData Data { get; set; }
    public Node3D TileNode { get; set; }
    
    public void InitUnit();
    
    public void Skill(Vector2I position, string skillName);
    public Dictionary<string, ISkill> Skills { get; set; }
    
    public void Move(TerrainInfo fromPos, TerrainInfo toPos, bool message = false);
    public IMovement Movement { get; set; }
    
    public void Damage(float amount);
    void Heal(float amount);

}