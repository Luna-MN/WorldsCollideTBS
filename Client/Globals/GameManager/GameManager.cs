using Godot;
using System;
using System.Collections.Generic;
using Util;

public partial class GameManager : Node
{
    public enum state
    {
        Entered,
        MainMenu,
        Login,
        Settings,
    }
    public Dictionary<state, string> stateScenes = new Dictionary<state, string>()
    {
        { state.Entered, "res://states/entered/entered.tscn" },
        { state.MainMenu, "res://states/MainMenu/mainMenu.tscn" },
        { state.Settings, "res://states/settings/settings.tscn" },
        { state.Login, "res://states/login/login.tscn" }
    };
    public ulong clientId;
    public string username;
    private Node CurrentSceneRoot;
    public void SetState(state newState)
    {
        if(CurrentSceneRoot != null) CurrentSceneRoot.QueueFree();
        
        var scene = (PackedScene)ResourceLoader.Load(stateScenes[newState]);

        CurrentSceneRoot = scene.Instantiate();
        AddChild(CurrentSceneRoot);
    }

}
