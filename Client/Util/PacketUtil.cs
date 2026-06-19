using Google.Protobuf.Collections;

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
    
    public static Packet NewSeedPacket(int seed)
    {
        return new Packet()
        {
            Seed = new SeedMessage()
            {
                Seed = { seed }
            }
        };
    }

    public static Packet NewGameVersionPacket(string version)
    {
        return new Packet()
        {
            GameVersion = new GameDataVersion()
            {
                Version = version
            }
        };
    }
    
    public static Packet NewIDPacket(ulong id)
    {
        return new Packet()
        {
            Id = new IdMessage()
            {
                Id = id
            }
        };
    }

    public static Packet NewArmyIdPacket(long id)
    {
        return new Packet()
        {
            ArmyId = new ArmyIDMessage()
            {
                Id = id
            }
        };
    }
}