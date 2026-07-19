
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

    public SkillType Type()
    {
        return SkillType.Attack;
    }

    public void Use(Vector2I position)
    {
        Globals.GM.CurrentGameData.GetTileAt(position).Unit.Damage(Data.Damage);
    }

    public void SendPacket(Vector2I position)
    {
        TrafficManager.Send(PacketUtil.NewSkillPacket(Name(), Type(), Unit.Data.UnitId, PacketUtil.WrapVec2I(position)));
    }

    public AttackData Data { get; set; }
}