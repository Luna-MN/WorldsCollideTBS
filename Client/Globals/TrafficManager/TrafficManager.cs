using Godot;
using System;
using Packets;

public partial class TrafficManager : Node
{
    public static Action<Packet> packetRecived;
    
    public static Error Send(Packet packet)
    {
        var err= Globals.WS.Send(packet);
        return err;
    }
    public static void Recieve(Packet packet)
    {
        GD.Print("hello");
        packetRecived?.Invoke(packet);
    }

    public static Packet NewChat(string message)
    {
        return new Packet {Chat = new ChatMessage {Msg = message}};
    }
    public static Packet NewId(ulong id)
    {
        return new Packet {Id = new IdMessage {Id = id}};
    }
}
