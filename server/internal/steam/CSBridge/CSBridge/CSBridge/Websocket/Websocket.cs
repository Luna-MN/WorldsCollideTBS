using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Packets;

namespace CSBridge.Websocket;

public enum WsError
{
    Ok = 0,
    Failed = 1,
    InvalidParameter = 2,
    AlreadyInUse = 3,
}

public enum WsState
{
    Connecting,
    Open,
    Closing,
    Closed,
}

public partial class Websocket : IDisposable
{
    private ClientWebSocket _socket = new();
    private CancellationTokenSource _cts = new();
    private WsState _lastState = WsState.Closed;
    private readonly ConcurrentQueue<Packet> _incoming = new();
    private Task _receiveLoop = Task.CompletedTask;

    public event Action ConnectedToServer;
    public event Action ConnectionClosed;

    public WsState State => _lastState;

    public async Task<WsError> ConnectToUrlAsync(string url, CancellationToken ct = default)
    {
        if (_lastState != WsState.Closed)
            return WsError.AlreadyInUse;

        try
        {
            _lastState = WsState.Connecting;
            await _socket.ConnectAsync(new Uri(url), ct);
            _lastState = WsState.Open;
            ConnectedToServer?.Invoke();

            _receiveLoop = Task.Run(ReceiveLoopAsync);
            return WsError.Ok;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connect failed: {ex.Message}");
            _lastState = WsState.Closed;
            return WsError.Failed;
        }
    }

    public async Task<WsError> SendAsync(Packet packet, CancellationToken ct = default)
    {
        if (_socket.State != WebSocketState.Open)
            return WsError.Failed;

        packet.SenderId = 0;
        var data = packet.ToByteArray();
        Console.WriteLine($"Packet sent: {packet.MsgCase}");

        try
        {
            await _socket.SendAsync(
                new ArraySegment<byte>(data),
                WebSocketMessageType.Binary,
                endOfMessage: true,
                cancellationToken: ct);
            return WsError.Ok;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Send failed: {ex.Message}");
            return WsError.Failed;
        }
    }

    /// <summary>Pulls the next received packet, or null if none are queued.</summary>
    public Packet GetPacket()
    {
        return _incoming.TryDequeue(out var p) ? p : null;
    }

    public int AvailablePacketCount => _incoming.Count;

    public async Task CloseAsync()
    {
        try
        {
            if (_socket.State == WebSocketState.Open)
            {
                await _socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
            }
        }
        catch { /* swallow during close */ }
        finally
        {
            _cts.Cancel();
            _lastState = WsState.Closed;
            ConnectionClosed?.Invoke();
        }
    }

    public void Clear()
    {
        _cts.Cancel();
        _socket.Dispose();
        _socket = new ClientWebSocket();
        _cts = new CancellationTokenSource();
        _lastState = WsState.Closed;
    }

    private async Task ReceiveLoopAsync()
    {
        var buffer = new byte[64 * 1024];
        try
        {
            while (!_cts.IsCancellationRequested && _socket.State == WebSocketState.Open)
            {
                using var ms = new System.IO.MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await _socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), _cts.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _lastState = WsState.Closed;
                        ConnectionClosed?.Invoke();
                        return;
                    }
                    ms.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                try
                {
                    var packet = Packet.Parser.ParseFrom(ms.ToArray());
                    _incoming.Enqueue(packet);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Bad packet: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException) { /* normal shutdown */ }
        catch (Exception ex)
        {
            Console.WriteLine($"Receive loop error: {ex.Message}");
        }
        finally
        {
            _lastState = WsState.Closed;
            ConnectionClosed?.Invoke();
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _socket.Dispose();
        _cts.Dispose();
    }
}