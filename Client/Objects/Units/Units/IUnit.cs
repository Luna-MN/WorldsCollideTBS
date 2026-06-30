using System.Collections.Generic;
using Godot;

public interface IUnit
{

    public Vector3 Position { get; set; }
    public Vector2I PositionI { get; set; }
    public UnitData Data { get; set; }
    public Node3D TileNode { get; set; }
    
    public void InitUnit();
    
    public void Attack(IUnit unit);
    public List<IAttack> Attacks { get; set; }
    
    public void Move(TerrainInfo fromPos, TerrainInfo toPos, bool message = false);
    public IMovement Movement { get; set; }

}