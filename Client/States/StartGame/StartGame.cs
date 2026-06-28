using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Packets;
using Packets.Util;

public partial class StartGame : Node3D, IState
{
    [Export]
    public Log log { get; set; }
    [Export]
    private PackedScene unitScene, terrainScene;
    [Export] private HNode3d unitTerrainNodes;
    public bool IsSmoothState => false;
    public Node[] TransitionNodes { get; set; }
    public int selectedSeed;
    [Export]
    private TerrainGen terrainGen1, terrainGen2, terrainGen3, selectedGen;
    [Export]
    private CustomTerrainInfo LeftSide, RightSide;
    [Export] private InputHandler inputHandler;
    [Export] public Button confirm;
    private List<CustomTerrainInfo> terrainList;
    public bool seedSelected;
    public override void _Ready()
    {
        Globals.GM.Subscribe(OnPacketReceived, OnWSConnectionClosed);
        confirm.Visible = false;
        confirm.ButtonUp += confirmClicked;
    }
    

    public override void _Process(double delta)
    {

    }

    public void OnPacketReceived(Packet packet)
    {
        switch (packet.MsgCase)
        {
            case Packet.MsgOneofCase.Seed:
                switch (packet.Seed.Seed.Count)
                {
                    case 3:
                        ThreeSeedMessageReceived(packet.Seed);
                        break;
                    case 2:
                        TwoSeedMessageReceived(packet.Seed);
                        break;
                    case 1:
                        log.error("Seed is too short.");
                        break;
                }
                break;
            case Packet.MsgOneofCase.UnitIds:
                HandleUnitIdsMessage(packet.UnitIds);
                break;
            case Packet.MsgOneofCase.UnitPositions:
                HandleUnitPositionsMessage(packet.SenderId, packet.UnitPositions);
                break;
        }
    }
    private void ThreeSeedMessageReceived(SeedMessage msg)
    {
        if (msg.Seed.Count < 3)
        {
            log.error("Seed is too short.");
            return;
        }
        terrainGen1.seed = msg.Seed[0];
        terrainGen2.seed = msg.Seed[1];
        terrainGen3.seed = msg.Seed[2];
        terrainGen1._Ready();
        terrainGen2._Ready();
        terrainGen3._Ready();
    }
    private void TwoSeedMessageReceived(SeedMessage seedMessage)
    {
        log.info("Seed received.");
        if (Globals.GM.CurrentGameData.MySide == CurrentGameData.Side.left)
        {
            Globals.GM.CurrentGameData.RightSeed = seedMessage.Seed[0];
        }
        else
        {
            Globals.GM.CurrentGameData.LeftSeed = seedMessage.Seed[0];
        }
        
        log.info($"Seeds: {Globals.GM.CurrentGameData.LeftSeed} {Globals.GM.CurrentGameData.RightSeed}");
        Globals.GM.CurrentGameData.GameSeed = seedMessage.Seed[1];
    }
    private void HandleUnitIdsMessage(UnitIDsMessage packetUnitIds)
    {
        Globals.GM.CurrentGameData.InitEnemyArmy(unitScene, packetUnitIds);
        GD.Print(Globals.GM.CurrentGameData.EnemyUnits.Count);
    }
    private void HandleUnitPositionsMessage(ulong senderId, UnitPositionsMessage packetUnitPositions)
    {
        log.info(senderId + " sent unit positions");
        if (senderId != Globals.GM.clientId)
        {
            foreach (var unit in packetUnitPositions.Units)
            {
                Globals.GM.CurrentGameData.EnemyUnits[unit.UnitId].PositionI = new Vector2I(unit.Position.X, unit.Position.Y);
            }
            log.info("Units received");
            selectedGen.Scale = new Vector3(1, 1, 1);
            foreach (var tile in selectedGen.worldInfo.TerrainInfo.Values)
            {
                if (tile.Unit != null)
                {
                    tile.Unit.Position = tile.Unit.TileNode.Position + new Vector3(0, tile.TileHeight, 0);
                        
                }

            }
            Globals.GM.SetState(GameManager.state.Game);
        }
    }
    private void confirmClicked()
    {
        if (selectedSeed == 0) return;
        if (!seedSelected)
        {
            if (Globals.GM.CurrentGameData.MySide == CurrentGameData.Side.left)
            {
                Globals.GM.CurrentGameData.LeftSeed = selectedSeed;
            }
            else
            {
                Globals.GM.CurrentGameData.RightSeed = selectedSeed;
            }

            seedSelected = true;
            TrafficManager.Send(PacketUtil.NewSeedPacket(selectedSeed));
            HideSeeds();
            CreateUnits();
            confirm.Visible = false;
            CustomTerrainInfo terrainSide = null;
            if (Globals.GM.CurrentGameData.MySide == CurrentGameData.Side.left)
            {
                LeftSide.Visible = true;
                terrainSide = LeftSide;
            }
            else
            {
                RightSide.Visible = true;
                terrainSide = RightSide;
            }
            var tw = terrainSide.CreateTween();
            tw.TweenProperty(terrainSide, "scale", new Vector3(1, 1, 1), 0.5f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
        }
        else
        {
            var unitsMessages = new List<UnitPositionMessage>();
            foreach (var unit in Globals.GM.CurrentGameData.MyUnits)
            {
                var message = PacketUtil.NewUnitPositionMessage(unit.Value.Data.UnitId, unit.Value.PositionI);
                unitsMessages.Add(message);
            }

            var n = new Node3D();
            AddChild(n);
            TransitionNodes = [n];
            selectedGen.GetParent().RemoveChild(selectedGen);
            n.CallDeferred("add_child", selectedGen);
            selectedGen.Name = "TerrainGen";
            foreach (var unit in Globals.GM.CurrentGameData.MyUnits.Values)
            {
                (unit as Node3D).GetParent().RemoveChild(unit as Node3D);
                n.CallDeferred("add_child", unit as Node3D);
                (unit as Node3D).Name = unit.Data.UnitName + " " + unit.Data.UnitId;
            }
            log.info("Sending unit positions");
            TrafficManager.Send(PacketUtil.NewUnitPositionsPacket(unitsMessages));
        }
    }
    // after hide seeds create a unit for each unit in my units, and then add them to HNode3D, use universalUnit
    private void CreateUnits()
    {
        var data = Globals.GM.CurrentGameData.MyUnitData;
        // create the terrains before creating the units
        terrainList = [];
        for (int i = 0; i < data.Count; i++)
        {
            var terrain = terrainScene.Instantiate<CustomTerrainInfo>();
            unitTerrainNodes.AddChild(terrain);
            terrainList.Add(terrain);
        }
        foreach (var unitData in data)
        {
            var unit = unitScene.Instantiate<UniversalUnit>();
            var unitIndex = data.IndexOf(unitData);
            unit.Data = unitData;
            AddChild(unit);
            var terrain = terrainList[unitIndex];
            unit.Position = new Vector3(terrain.GlobalPosition.X, terrain.TerrainInfo.TileHeight, terrain.GlobalPosition.Z);
            Globals.GM.CurrentGameData.MyUnits.Add(unit.Data.UnitId, unit);
            terrain.TerrainInfo.Unit = unit;
        }
        
    }
    public bool UnassignedUnits()
    {
        foreach (var terrain in terrainList)
        {
            if (terrain.TerrainInfo.Unit != null)
            {
                return true;
            }
        }
        return false;
    }
    
    private async void HideSeeds()
    {
        TerrainGen[] gens = [terrainGen1, terrainGen2, terrainGen3];
        Tween tween = null;
        List<TerrainGen> toDestroy = [];
        foreach (var gen in gens)
        {
            var tw = gen.CreateTween();
            if (gen.seed == selectedSeed)
            {
                tw.TweenProperty(gen, "scale", new Vector3(1.25f, 1.25f, 1.25f), 0.2f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
                tween = tw;
                selectedGen = gen;
            }
            else
            {
                tw.TweenProperty(gen, "scale", new Vector3(0.001f, 0.001f, 0.001f), 0.2f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
                toDestroy.Add(gen);
            }
        }
        await tween.ToSignal(tween, Tween.SignalName.Finished);
        foreach (var gen in toDestroy)
        {
            gen.QueueFree();
        }
        tween = CreateTween();
        tween.TweenProperty(selectedGen, "position", new Vector3(0, 0, 0), 0.2f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
    }
    public void GrabFocus()
    {
        TerrainGen[] gens = [terrainGen1, terrainGen2, terrainGen3];
        foreach (var gen in gens)
        {
            var tw = gen.CreateTween();
            if (gen.seed == selectedSeed)
            {
                tw.TweenProperty(gen, "scale", new Vector3(1.25f, 1.25f, 1.25f), 0.2f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
            }
            else
            {
                tw.TweenProperty(gen, "scale", new Vector3(0.75f, 0.75f, 0.75f), 0.2f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
            }
        }
    }

    public void ResetFocus()
    {
        TerrainGen[] gens = [terrainGen1, terrainGen2, terrainGen3];
        foreach (var gen in gens)
        {
            var tw = gen.CreateTween();
            tw.TweenProperty(gen, "scale", new Vector3(1f, 1f, 1f), 0.2f).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
        }
    }
    public void OnWSConnectionClosed()
    {
        throw new NotImplementedException();
    }
    public override void _ExitTree()
    {
        Globals.GM.Unsubscribe(OnPacketReceived, OnWSConnectionClosed);
    }
}
