
using Godot;
using Packets;
using Packets.Util;

public class BasicAttack : IAttack
{
    public IUnit Unit { get; set; }
    public string Name()
    {
        return "BasicAttack";
    }

    public Node3D Node { get; set; }
    public void Init(IUnit unit, Node3D node, SkillData data)
    {
        Node = node;
        Unit = unit;
        Data = data as AttackData;
    }
    

    public AttackData Data { get; set; }

    public void Attack(IUnit unit)
    {
        // Do attack Logic
        unit.Damage(Data.Damage);
    }

    public void SendAttackPacket(IUnit unit)
    {
        TrafficManager.Send(PacketUtil.NewSkillPacket(Name(), SkillType.Attack, Unit.Data.UnitId, unit.Data.UnitId));
    }
}