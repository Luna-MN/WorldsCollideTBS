using System.Collections.Generic;
using Godot;
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

    public static Packet NewUnitPositionsPacket(List<UnitPositionMessage> unitPositions)
    {
        var p = new Packet
        {
            UnitPositions = new UnitPositionsMessage()
        };
        p.UnitPositions.Units.AddRange(unitPositions);
        return p;
    }

    public static UnitPositionMessage NewUnitPositionMessage(long id, Vector2I pos)
    {
        return new UnitPositionMessage()
        {
            UnitId = id,
            Position = new Vector2IMsg()
            {
                X = pos.X,
                Y = pos.Y
            }
        };
    }

    public static Packet NewUnitPositionPacket(long id, Vector2I pos)
    {
        return new Packet()
        {
            UnitPosition = new UnitPositionMessage()
            {
                UnitId = id,
                Position = new Vector2IMsg()
                {
                    X = pos.X,
                    Y = pos.Y
                }
            }
        };
    }
}