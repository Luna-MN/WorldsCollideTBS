using Godot;
using System;
using System.Linq;
using Packets;
using Packets.Util;

public partial class MainGame : Node3D, ISmoothState
{
    [Export]
    public Log log { get; set; }
    public Node[] PrevObjects { get; set; }
    public bool IsSmoothState => true;
    public Node[] TransitionNodes { get; set; }
    [Export] private TerrainGen TerrainGen1, MainGameTerrainGen, TerrainGen2;

    [Export] public Ui UI;

    public override void _Ready()
    {
        Globals.GM.Subscribe(OnPacketReceived, OnWSConnectionClosed);
        
        UI.EndTurn.ButtonUp += EndTurnClicked;
        
        TerrainGen1.seed = (int)Globals.GM.CurrentGameData.LeftSeed;
        TerrainGen2.seed = (int)Globals.GM.CurrentGameData.RightSeed;
        MainGameTerrainGen.seed = (int)Globals.GM.CurrentGameData.GameSeed;
        TerrainGen1._Ready();
        TerrainGen2._Ready();
        MainGameTerrainGen._Ready();
        CallDeferred(nameof(SceneInitializeDeferred));
    }

    private void SceneInitializeDeferred()
    {
        var n = (PrevObjects[0] as Node3D);
        if (Globals.GM.CurrentGameData.MySide == CurrentGameData.Side.left)
        {
            TerrainGen1.QueueFree();
            TerrainGen1 = null;
            TerrainGen1 = n.GetNode<TerrainGen>("TerrainGen");
            n.Position = new Vector3(-13.8f, 0, 0);
        }
        else
        {
            TerrainGen2.QueueFree();
            TerrainGen2 = null;
            TerrainGen2 = n.GetNode<TerrainGen>("TerrainGen");
            n.Position = new Vector3(13.8f, 0, 0);
        }
        
        Globals.GM.CurrentGameData.TerrainGen = MainGameTerrainGen;
        Globals.GM.CurrentGameData.TerrainGen1 = TerrainGen1;
        Globals.GM.CurrentGameData.TerrainGen2 = TerrainGen2;
        Globals.GM.CurrentGameData.UpdateTileNeighbours();
        var myT = n.GetNode<TerrainGen>("TerrainGen");
        n.RemoveChild(myT);
        foreach (var unit in Globals.GM.CurrentGameData.MyUnits.Values)
        {
            n.RemoveChild(unit as Node3D);
        }
        CallDeferred(nameof(AddBackDeferred), myT);
        InitEnemyUnits();
    }

    private void InitEnemyUnits()
    {
        foreach (var unit in Globals.GM.CurrentGameData.EnemyUnits.Values)
        {
            var nodeU = (unit as Node3D);
            Globals.GM.CurrentGameData.Tiles[unit.PositionI].Unit = unit;
            AddChild(nodeU);
            var uTile = Globals.GM.CurrentGameData.Tiles[unit.PositionI];
            nodeU.Position = uTile.Position + new Vector3(0, uTile.TileHeight, 0);
        }
    }

    private void AddBackDeferred(TerrainGen myT)
    {
        AddChild(myT);
        if (Globals.GM.CurrentGameData.MySide == CurrentGameData.Side.left)
        {
            myT.Position = new Vector3(-13.8f, 0, 0);
        }
        else
        {
            myT.Position = new Vector3(13.8f, 0, 0);
        }

        foreach (var unit in Globals.GM.CurrentGameData.MyUnits.Values)
        {
            var nodeU = (unit as Node3D);
            AddChild(nodeU);
        }

        foreach (var tile in myT.worldInfo.TerrainInfo.Values)
        {
            if (tile.Unit != null)
            {
                tile.Unit.Position = tile.Unit.TileNode.GlobalPosition + new Vector3(0, tile.TileHeight, 0);
                        
            }

        }
    }

    private void EndTurnClicked()
    {
        GD.Print("End Turn Clicked");
        TrafficManager.Send(PacketUtil.NewEndTurnPacket());
    }
    public void OnPacketReceived(Packet packet)
    {
        switch (packet.MsgCase)
        {
            case Packet.MsgOneofCase.IDs:
                HandleTurnOrder(packet);
                break;
            case Packet.MsgOneofCase.Turn:
                HandleTurnChange(packet);
                break;
            case Packet.MsgOneofCase.HexPositions:
                HandleHexPositions(packet);
                break;
        }
    }

    private void HandleTurnOrder(Packet packet)
    {
        Globals.GM.CurrentGameData.TurnOrder = packet.IDs.IDs.Select(id => id.Id).ToArray();
    }
    
    private void HandleTurnChange(Packet packet)
    {
        if (!Globals.GM.CurrentGameData.TurnOrder.Contains(packet.Turn.Id))
        {
            log.error("Turn change received but not in turn order");
            return;
        }
        Globals.GM.CurrentGameData.currTurnPointer = Globals.GM.CurrentGameData.TurnOrder.ToList().IndexOf(packet.Turn.Id);
        Globals.GM.CurrentGameData.MyTurn = Globals.GM.CurrentGameData.TurnOrder[Globals.GM.CurrentGameData.currTurnPointer] == Globals.GM.clientId;
        UI.EndTurn.Visible = Globals.GM.CurrentGameData.MyTurn;
    }
    
    private void HandleHexPositions(Packet packet)
    {
        if (packet.SenderId == Globals.GM.opponentId)
        {
            var unit = Globals.GM.CurrentGameData.EnemyUnits[packet.HexPositions.Id];
            var toPos = Globals.GM.CurrentGameData.GetTileAt(PacketUtil.UnwrapVec2I(packet.HexPositions.Positions[^1].Position));
            var fromPos = Globals.GM.CurrentGameData.GetTileAt(PacketUtil.UnwrapVec2I(packet.HexPositions.Positions[0].Position));
            unit.Move(fromPos, toPos, true);
        }
        else
        {
            var unit = Globals.GM.CurrentGameData.MyUnits[packet.HexPositions.Id];
            var toPos = Globals.GM.CurrentGameData.GetTileAt(PacketUtil.UnwrapVec2I(packet.HexPositions.Positions[^1].Position));
            var fromPos = Globals.GM.CurrentGameData.GetTileAt(PacketUtil.UnwrapVec2I(packet.HexPositions.Positions[0].Position));
            if (unit.Movement.ValidateMovement(packet.HexPositions))
            {
                unit.Move(toPos, fromPos);
            }
        }
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
