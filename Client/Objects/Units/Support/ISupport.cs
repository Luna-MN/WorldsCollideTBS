using Godot;

public interface ISupport : ISkill
{
    public void Support(IUnit unit);
    public void SendSupportPacket(IUnit unit);
}