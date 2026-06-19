using Godot;
using System;
using Packets;

public partial class LoginAdmin : Node, IState
{
    private Action ActionOnOkReceived;

    [Export] public Log log { get; set; }
    public bool IsSmoothState => false;
    public Node[] TransitionNodes { get; set; }
    [Export] private LineEdit Username, Password;
    [Export] private Button LoginButton, RegisterButton;
    private GameDataUpdater gameDataUpdater;
    
    public override void _Ready()
    {
        Globals.GM.Subscribe(OnPacketReceived, OnWSConnectionClosed);
        SendVersion();
        LoginButton.Pressed += OnLoginPressed;
        RegisterButton.Pressed += OnRegisterPressed;
    }
    private void SendVersion()
    {
        gameDataUpdater = new GameDataUpdater();
        TrafficManager.Send(Packets.Util.PacketUtil.NewGameVersionPacket(gameDataUpdater.GetVersion()));
    }

    private void OnLoginPressed()
    {
        var packet = new Packet();
        packet.LoginRequest = new LoginRequestMessage {Username = Username.Text, Password = Password.Text};
        ActionOnOkReceived = () =>
        {
            log.success("Logged in successfully.");
            Globals.GM.username = Username.Text;
            Globals.GM.SetState(GameManager.state.MainLoggedInMenu);
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

    public void OnPacketReceived(Packet packet)
    {
        GD.Print(packet);
        var senderId = packet.SenderId;
        switch (packet.MsgCase)
        {
            case Packet.MsgOneofCase.Deny:
                log.error("Login failed." + packet.Deny.Reason);
                break;
            case Packet.MsgOneofCase.GameData:
                HandleGameData(packet.GameData);
                break;
            case Packet.MsgOneofCase.OK:
                ActionOnOkReceived?.Invoke();
                ActionOnOkReceived = null;
                break;
        }
    }
    private void HandleGameData(GameDataMessage msg)
    {
        gameDataUpdater.UpdateVersion(msg.Version.Version, msg);
    }
    public void OnWSConnectionClosed()
    {
        log.warning("Connection closed.");
    }
    public override void _ExitTree()
    {
        Globals.GM.Unsubscribe(OnPacketReceived, OnWSConnectionClosed);
    }
}
