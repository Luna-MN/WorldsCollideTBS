using Godot;
using System;
using System.Collections.Generic;
using Packets;
using Util;

public partial class GameManager : Node
{
    public enum state
    {
        Connect,
        MainMenu,
        Login,
        LoginAdmin,
        Settings,
        MainLoggedInMenu,
        Lobby,
        StartGame,
        AwaitingGameData,
        Game
    }
    public Dictionary<state, string> stateScenes = new Dictionary<state, string>()
    {
        { state.Connect, "res://States/Connect/Connect.tscn" },
        { state.MainMenu, "res://States/MainMenu/MainMenu.tscn" },
        { state.Settings, "res://States/settings/settings.tscn" },
        { state.Login, "res://States/login/login.tscn" },
        { state.LoginAdmin, "res://States/LoginAdmin/LoginAdmin.tscn" },
        { state.MainLoggedInMenu, "res://States/Menus/MainLoggedInMenu/MainLoggedInMenu.tscn"},
        { state.Lobby, "res://States/Lobby/Lobby.tscn"},
        { state.StartGame, "res://States/StartGame/StartGame.tscn"},
        { state.AwaitingGameData, "res://States/AwaitingGameData/AwaitingGameData.tscn"},
        { state.Game, "res://States/MainGame/MainGame.tscn" }
    };
    public ulong clientId;
    public string username;
    private Node CurrentSceneRoot;
    public GameData gameData;
    public void SetState(state newState)
    {
        var scene = (PackedScene)ResourceLoader.Load(stateScenes[newState]);
        var nextScene = scene.Instantiate();
        if (CurrentSceneRoot != null)
        {
            Unsubscribe(((IState)CurrentSceneRoot).OnPacketReceived, ((IState)CurrentSceneRoot).OnWSConnectionClosed);
        }
        if (((IState)nextScene).IsSmoothState)
        { 
            var s = ((ISmoothState)nextScene);
             s.PrevObjects = ((IState)CurrentSceneRoot).TransitionNodes;
             foreach (var obj in s.PrevObjects)
             {
                 obj.GetParent().RemoveChild(obj);
                 nextScene.CallDeferred("add_child", obj);
             }
        }
        else
        {
            GD.Print("not smooth state");
        }
        if(CurrentSceneRoot != null) CurrentSceneRoot.QueueFree();
        

        GD.Print($"[State] {newState} {scene.ResourcePath}");

        CurrentSceneRoot = nextScene;
        GetTree().Root.GetNode("Main").AddChild(CurrentSceneRoot);
    }

    public void Subscribe(Action<Packet> callback, Action Disconnect)
    {
        TrafficManager.packetReceived += callback;
        Globals.WS.connectionClosed += Disconnect;
    }
    public void Unsubscribe(Action<Packet> callback, Action Disconnect)
    {
        TrafficManager.packetReceived -= callback;
        Globals.WS.connectionClosed -= Disconnect;
    }

    public void Unsubscribe()
    {
        
    }
    // Opponent Data
    public ulong opponentId;
    public string opponentUserString;
    public bool opponentIsSteamClient;
}
