using System.Collections.Generic;

public interface IUnit
{
    public void InitUnit();
    
    public void Attack(IUnit unit);
    public IAttack Attacks { get; set; }
    
    public void Move(TerrainInfo fromPos, TerrainInfo toPos);
    public IMovement Movement { get; set; }

}