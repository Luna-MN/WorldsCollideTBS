using Godot;
using System;
using Google.Protobuf;
using Packets;

public partial class Websocket : Node
{
    private WebSocketPeer socket = new WebSocketPeer();
    private WebSocketPeer.State lastState = WebSocketPeer.State.Closed;
    
    public event Action connectedToServer;
    public event Action connectionClosed;
    
    public Error connectToUrl(string url, TlsOptions tlsOptions)
    {
        var err = socket.ConnectToUrl(url, tlsOptions);
        if (err != Error.Ok) return err;
        
        lastState = socket.GetReadyState();
        return Error.Ok;
    }

    public Error Send(Packet packet)
    {
        packet.SenderId = 0;
        var data = packet.ToByteArray();
        GD.Print("packet Sent " + packet.MsgCase);
        GD.Print(data);
        return socket.Send(data);
    }

    private Packet GetPacket()
    {
        if (socket.GetAvailablePacketCount() < 1)
        {
            return null;
        }
        
        var data = socket.GetPacket();
        var packet = Packet.Parser.ParseFrom(data);
        
        return packet;
    }
    
    public void Close()
    {
        socket.Close();
        lastState = socket.GetReadyState();
    }

    public void clear()
    {
        socket = new WebSocketPeer();
        lastState = socket.GetReadyState();
    }
    
    public WebSocketPeer GetSocket()
    {
        return socket;
    }

    public void poll()
    {
        if (socket.GetReadyState() != WebSocketPeer.State.Closed)
        {
            socket.Poll();
        }

        var state = socket.GetReadyState();

        if (lastState != state)
        {
            lastState = state;
            if (state == WebSocketPeer.State.Open)
            {
                connectedToServer?.Invoke();
            }
            else if (state == WebSocketPeer.State.Closed)
            {
                connectionClosed?.Invoke();
            }
        }
        while (socket.GetReadyState() == WebSocketPeer.State.Open && socket.GetAvailablePacketCount() > 0)
        {
            TrafficManager.Recieve(GetPacket());
        }
    }

    public override void _Process(double delta)
    {
        poll();
    }
}
