using Godot;
using System;
using Packets;
using Packets.Util;

public partial class ChooseArmy : Control, IState
{
    [Export]
    private PackedScene armyScene;
    [Export]
    private GridContainer armyGrid;
    [Export]
    private ScrollContainer armyScroll;

    [Export] private RichTextLabel waitPlease;
    
    public Log log { get; set; }
    public bool IsSmoothState => false;
    public Node[] TransitionNodes { get; set; }

    public override void _Ready()
    {
        Globals.GM.Subscribe(OnPacketReceived, OnWSConnectionClosed);
        CreateArmies();
    }

    private void CreateArmies()
    {
        var facs = Globals.GDH.GetFactions();
        foreach (var fac in facs)
        {
            var armies = Globals.GDH.GetArmiesForFaction(fac.ID);
            foreach (var army in armies.Values)
            {
                // create node
                var node = armyScene.Instantiate<ArmyButton>();
                node.SetArmyName(army.Name);
                node.SetArmyFaction(fac.Name);
                node.SetArmyDescription(army.Description);
                node.ButtonUp += () =>
                {
                    Globals.GM.CurrentGameData.MyArmyID = army.ID;
                    StartGame();
                    armyScroll.Visible = false;
                    waitPlease.Visible = true;
                };
                armyGrid.AddChild(node);
            }
        }
    }
    private void StartGame()
    {
        TrafficManager.Send(PacketUtil.NewArmyIdPacket(Globals.GM.CurrentGameData.MyArmyID));
    }
    public void OnPacketReceived(Packet packet)
    {
        switch (packet.MsgCase)
        {
            case Packet.MsgOneofCase.ArmyId:
                HandleArmyId(packet.SenderId, packet.ArmyId);
                break;
            case Packet.MsgOneofCase.OK:
                HandleOKMessage();
                break;
        }
    }

    private void HandleArmyId(ulong id, ArmyIDMessage packetArmyId)
    {
        if (id == Globals.GM.clientId)
        {
            Globals.GM.CurrentGameData.MyArmyID = packetArmyId.Id;
        }
        else
        {
            Globals.GM.CurrentGameData.EnemyArmyID = packetArmyId.Id;
        }


    }

    private void HandleOKMessage()
    {
        GD.Print(Globals.GM.CurrentGameData.GetMyArmy().Name);
        Globals.GM.SetState(GameManager.state.StartGame);   
    }
    
    public void OnWSConnectionClosed()
    {
        Globals.GM.SetState(GameManager.state.MainMenu);  
    }
    public override void _ExitTree()
    {
        Globals.GM.Unsubscribe(OnPacketReceived, OnWSConnectionClosed);
    }   
}
