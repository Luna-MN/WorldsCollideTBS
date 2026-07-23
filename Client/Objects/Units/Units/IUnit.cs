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

    void NewTurn();

    void AddToSkillBuffer(int skillId, ICombatAction action);
    void RemoveFromSkillBuffer(int skillId);
    
    void Inflict(int skillId, ICombatAction action, int Turns);
    void RemoveInflict(int skillId);
    
    void Damage(float amount);
    void Heal(float amount);

}

public class InflictAction
{
    public ICombatAction Action;
    public int Turns;
}