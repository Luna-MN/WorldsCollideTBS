using Godot;
using System;
using Packets.Util;

public partial class CustomMatch : Button
{
    public override void _Ready()
    {
        ButtonUp += () =>
        {
            Globals.GM.SetState(GameManager.state.Lobby);
            Globals.WS.Send(PacketUtil.NewChangeStatePacket("Lobby"));
        };
    }
}
