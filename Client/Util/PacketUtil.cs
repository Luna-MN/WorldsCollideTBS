namespace Packets.Util;

public static class PacketUtil
{
    public static Packet NewQueuePacket(string queueType)
    {
        return new Packet()
        {
            Queue = new QueueMessage()
            {
                QueueType = queueType
            }
        };
    }

    public static Packet NewChangeStatePacket(string state)
    {
        return new Packet()
        {
            ChangeState = new ChangeStateMessage()
            {
                StateName = state
            }
        };
    }
}