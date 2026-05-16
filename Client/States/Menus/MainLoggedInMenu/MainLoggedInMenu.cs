using Godot;
using System;
using Packets;

public partial class MainLoggedInMenu : Control, IState
{
    [Export]
    public Log log { get; set; }
    [Export]
    private NavagationButton[] navButtons;
    private NavagationButton currentButton;
    private MenuPeice CurrentScene;
    public override void _Ready()
    {
        foreach (NavagationButton button in navButtons)
        {
            button.ButtonUp += () =>
            {
                if (button.Scene == null)
                {
                    log.error("Not implemented");    
                    return;
                }
                ChangeScene(button.Scene);
                if (currentButton != null)
                {
                    currentButton.Disabled = false;
                }
                button.Disabled = true;
                currentButton = button;
            };
        }
        ChangeScene(navButtons[0].Scene);
        navButtons[0].Disabled = true;
    }

    public void OnPacketReceived(Packet packet)
    {
        throw new NotImplementedException();
    }

    public void OnWSConnectionClosed()
    {
        Globals.GM.SetState(GameManager.state.MainMenu);
    }

    private void ChangeScene(PackedScene scene)
    {
        if (CurrentScene != null)
        {
            CurrentScene.QueueFree();
        }
        CurrentScene = scene.Instantiate<MenuPeice>();
        AddChild(CurrentScene);
    }
}
