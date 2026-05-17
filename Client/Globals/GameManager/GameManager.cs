using Godot;
using System;
using System.Collections.Generic;
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
        Lobby
    }
    public Dictionary<state, string> stateScenes = new Dictionary<state, string>()
    {
        { state.Connect, "res://States/Connect/Connect.tscn" },
        { state.MainMenu, "res://States/MainMenu/MainMenu.tscn" },
        { state.Settings, "res://States/settings/settings.tscn" },
        { state.Login, "res://States/login/login.tscn" },
        { state.LoginAdmin, "res://States/LoginAdmin/LoginAdmin.tscn" },
        { state.MainLoggedInMenu, "res://States/Menus/MainLoggedInMenu/MainLoggedInMenu.tscn"},
        { state.Lobby, "res://States/Lobby/Lobby.tscn"}
    };
    public ulong clientId;
    public string username;
    private Node CurrentSceneRoot;
    public void SetState(state newState)
    {
        if(CurrentSceneRoot != null) CurrentSceneRoot.QueueFree();
        
        var scene = (PackedScene)ResourceLoader.Load(stateScenes[newState]);
        GD.Print($"[State] {newState} {scene.ResourcePath}");

        CurrentSceneRoot = scene.Instantiate();
        GetTree().Root.GetNode("Main").AddChild(CurrentSceneRoot);
    }

}
