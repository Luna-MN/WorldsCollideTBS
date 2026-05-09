using CSBridge.Websocket;
using Packets;

using var ws = new Websocket();

ws.ConnectedToServer += () => Console.WriteLine("Connected!");
ws.ConnectionClosed  += () => Console.WriteLine("Disconnected.");

var err = await ws.ConnectToUrlAsync("ws://localhost:8080");
if (err != WsError.Ok)
{
    Console.WriteLine($"Connection failed: {err}");
    return;
}

// Example: send a login packet
await ws.SendAsync(new Packet
{
    LoginRequest = new LoginRequestMessage { Username = "alice", Password = "secret" }
});

// Drain incoming packets for a few seconds
var until = DateTime.UtcNow.AddSeconds(5);
while (DateTime.UtcNow < until)
{
    while (ws.GetPacket() is { } p)
        Console.WriteLine($"Received: {p.MsgCase}");
    await Task.Delay(50);
}

await ws.CloseAsync();