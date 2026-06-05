using Godot;
using System;
using System.Collections.Generic;

public partial class DefaultUnit : Node3D, IUnit
{
    public IMovement Movement { get; set; }
    public IAttack Attacks { get; set; }

    public override void _Ready()
    {
        InitUnit();
    }
    public virtual void InitUnit()
    {
        Movement = new BasicMovement();
        Movement.InitMovement(this, this);
        // Attacks.InitAttack(this, this);
    }
    
    
    public virtual void Move(TerrainInfo fromPos, TerrainInfo toPos)
    {
        var path = PathFinding.FindCheapestPath(fromPos, toPos);
        Movement.Move(path);
    }
    public virtual void Attack(IUnit unit)
    {
        Attacks.Attack(unit);
    }

}
