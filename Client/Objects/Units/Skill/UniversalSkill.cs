using Godot;
using Packets;
using Packets.Util;


public class UniversalSkill : ISkill
{
    public IUnit Unit { get; set; }
    public ICombatAction CombatAction { get; set; }
    public SkillDataJSON Data { get; set; }
    public virtual string Name()
    {
        return "UniversalSkill";
    }

    public Node3D Node { get; set; }
    public virtual void Init(IUnit unit, Node3D node, SkillDataJSON data)
    {
        Node = node;
        Unit = unit;
        Data = data;
    }

    public virtual void CombatString(string str)
    {
        CombatAction = Globals.GM.SkillCompiler.Compile(str);
    }
    
    public virtual SkillType Type()
    {
        return Data.Type;
    }

    public virtual void Use(Vector2I position)
    {
        var skillId = Globals.GM.CurrentGameData.CurrentSkillID;
        CombatAction.Execute(new CombatContext(Unit, GetUnitAt(position)), skillId);
        // we need to send the skillId with the packet
        SendPacket(position, skillId);
    }

    protected IUnit GetUnitAt(Vector2I position)
    {
        return Globals.GM.CurrentGameData.GetTileAt(position).Unit;
    }
    public virtual void SendPacket(Vector2I position, int skillId)
    {
        TrafficManager.Send(PacketUtil.NewSkillPacket(skillId, Name(), Type(), Unit.Data.UnitId, PacketUtil.WrapVec2I(position)));
    }
}