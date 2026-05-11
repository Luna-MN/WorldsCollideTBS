using Godot;
using System;
using Packets;

public partial class LoginAdmin : Node, IState
{
    private Action ActionOnOkReceived;

    [Export] public Log log { get; set; }
    [Export] private LineEdit Username, Password;
    [Export] private Button LoginButton, RegisterButton;

    public override void _Ready()
    {
        TrafficManager.packetRecived += OnPacketReceived;
        Globals.WS.connectionClosed += OnWSConnectionClosed;

        LoginButton.Pressed += OnLoginPressed;
        RegisterButton.Pressed += OnRegisterPressed;
    }
    private void OnLoginPressed()
    {
        var packet = new Packet();
        packet.LoginRequest = new LoginRequestMessage {Username = Username.Text, Password = Password.Text};
        ActionOnOkReceived = () =>
        {
            log.success("Logged in successfully.");
            Globals.GM.username = Username.Text;
        };

        TrafficManager.Send(packet);

    }
    private void OnRegisterPressed()
    {
        var packet = new Packet();
        packet.RegisterRequest = new RegisterRequestMessage {Username = Username.Text, Password = Password.Text};
        ActionOnOkReceived = () => log.success("Registered successfully.");

        TrafficManager.Send(packet);

    }

    public void OnPacketReceived(Packet obj)
    {
        GD.Print(obj);
        var senderId = obj.SenderId;
        if (obj.MsgCase == Packet.MsgOneofCase.Deny)
        {
            log.error("Login failed." + obj.Deny.Reason);
        }
        else if (obj.MsgCase == Packet.MsgOneofCase.OK)
        {
            ActionOnOkReceived?.Invoke();
            ActionOnOkReceived = null;
        }
    }
    public void OnWSConnectionClosed()
    {
        log.warning("Connection closed.");
    }

}
