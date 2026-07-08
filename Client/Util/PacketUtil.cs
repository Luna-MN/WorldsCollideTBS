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

    public static UnitPositionMessage NewUnitPositionMessage(int id, Vector2I pos)
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

    public static Packet NewUnitPositionPacket(int id, Vector2I pos)
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

    public static Packet NewHexPositions(int unitId, List<HexPositionMessage> hexPositions)
    {
        return new Packet()
        {
            HexPositions = new HexPositionsMessage()
            {
                Id = unitId,
                Positions = { hexPositions }
            }
        };
    }
    public static HexPositionMessage NewHexPositionMessage(Vector2I pos)
    {
        return new HexPositionMessage()
        {
            Position = new Vector2IMsg()
            {
                X = pos.X,
                Y = pos.Y
            }
        };
    }

    public static Packet NewUnitIdsPacket(List<UIDData> data)
    {
        var p = new Packet();
        var UIDS = new UnitIDsMessage();

        foreach (var d in data)
        {
            var UID = new UnitIDMessage()
            {
                Id = d.UnitId,
                UnitId = d.Id
            };
            UIDS.Ids.Add(UID);
        }
        p.UnitIds = UIDS;
        return p;
    }

    public static Vector2I UnwrapVec2I(Vector2IMsg vec2)
    {
        return new Vector2I()
        {
            X = vec2.X,
            Y = vec2.Y
        };
    }

    public static Packet NewEndTurnPacket()
    {
        return new Packet()
        {
            Turn = new TurnMessage()
            {
                Id = Globals.GM.clientId
            }
        };
    }
}

public class UIDData
{
    public int Id { get; set; }
    public long UnitId { get; set; }
}