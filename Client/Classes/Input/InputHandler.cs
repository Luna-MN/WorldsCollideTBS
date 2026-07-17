using Godot;
using Util;

public partial class InputHandler : Node3D
{
    private Vector3 MousePosition
    {
        get
        {
            MousePos();
            return mousePosition;
        }
        set => mousePosition = value;
    }

    private Vector3 mousePosition;
    public Tile CurrentMouseNode;
    [Export] private bool StoreNode, CamMove;
    [Export] private Camera Camera;
    [Export] private float camSpeed;
    
    [ExportGroup("StartGame")] 
    [Export(PropertyHint.GroupEnable)] private bool StartGame;
    [Export] private StartGame startGame;
    [ExportGroup("MainGame")] 
    [Export(PropertyHint.GroupEnable)] private bool MainGame;
    [Export] private MainGame mainGame;

    private Tile SelectedTile;
    public override void _Ready()
    {
        if (Camera != null)
        {
            Camera.ResetPos = Camera.Position;
            Camera.ResetScale = Camera.Size;
        }
        RayCast.Set(GetTree(), GetViewport(), GetWorld3D());
    }

    public override void _Process(double delta)
    {
        MouseHover();
        if (StartGame) StartGameInputHandling();
        if (MainGame) MainGameInputHandling();
        if (Camera != null && CamMove) CameraInputHandling();
    }
    private void MousePos()
    {
        mousePosition = RayCast.CastPosition();
    }
    private void MouseHover()
    {
        var mouseObj = RayCast.CastObject<Node3D>();
        if (mouseObj?.GetParent().GetParent().GetParent() is Asteroid || mouseObj?.GetParent() is Asteroid)
        {
            return;
        }
        var obj = mouseObj?.GetParent<Tile>();
        
        if (obj == null)
        {
            if (!StoreNode)
            {
                if (CurrentMouseNode == null) return;
                CurrentMouseNode.Position = new Vector3(CurrentMouseNode.Position.X, 0, CurrentMouseNode.Position.Z);
                CurrentMouseNode = null;
                return;
            }
            if (CurrentMouseNode != null)
            {
                CurrentMouseNode.Position = new Vector3(CurrentMouseNode.Position.X, 0, CurrentMouseNode.Position.Z);
            }
            return;
        }
        if (obj == CurrentMouseNode) return;
        CurrentMouseNode ??= obj;

        CurrentMouseNode.Position = new Vector3(CurrentMouseNode.Position.X, 0, CurrentMouseNode.Position.Z);
        CurrentMouseNode = obj;
        CurrentMouseNode.Position = new Vector3(CurrentMouseNode.Position.X, 0.25f, CurrentMouseNode.Position.Z);
    }

    private void CameraInputHandling()
    {
        var pos = Vector3.Zero;
        if (Input.IsActionPressed("move_forward"))
        {
            pos += Vector3.Forward;
        }

        if (Input.IsActionPressed("move_backward"))
        {
            pos += Vector3.Back;
        }

        if (Input.IsActionPressed("move_left"))
        {
            pos += Vector3.Left;
        }

        if (Input.IsActionPressed("move_right"))
        {
            pos += Vector3.Right;
        }

        if (pos != Vector3.Zero)
        {
            Camera.Position += pos * camSpeed;
        }

        Camera.Position = Camera.Position.Clamp(new Vector3(-20, -100, -20), new Vector3(20, 100, 20));

        if (Input.IsActionJustPressed("ui_scroll_down"))
        {
            Camera.Size += 0.5f;
        }

        if (Input.IsActionJustPressed("ui_scroll_up"))
        {
            Camera.Size -= 0.5f;
        }

        if (Input.IsActionPressed("cam_reset"))
        {
            Camera.Position = Camera.ResetPos;
            Camera.Size = Camera.ResetScale;
        }
}
    private void StartGameInputHandling()
    {
        if (GetTree().Root.GuiGetFocusOwner() != null && !startGame.seedSelected)
        {
            return;
        }
        
        if (Input.IsActionJustPressed("ui_click"))
        {
            HandleStartGameLeftClick();
        }
    }
    private void HandleStartGameLeftClick()
    {
        if (!startGame.seedSelected)
        {
            if (CurrentMouseNode == null)
            {
                startGame.ResetFocus();
                startGame.selectedSeed = 0;
                startGame.confirm.Visible = false;
                return;
            }

            var node = CurrentMouseNode.GetParent<TerrainGen>();
            startGame.selectedSeed = node.seed;
            startGame.confirm.Visible = true;
            startGame.GrabFocus();
        }

        if (startGame.seedSelected)
        {
            if (SelectedTile == null && CurrentMouseNode?.TerrainInfo?.Unit != null)
            {
                SelectedTile = CurrentMouseNode;
                return;
            }
            StartGameMovement();
        }
        
    }
    private void StartGameMovement()
    {
        if (CurrentMouseNode == null || SelectedTile == null || CurrentMouseNode == SelectedTile)
        {
            return;
        }
        var currNode = SelectedTile.TerrainInfo;
            
        var moveToNode = CurrentMouseNode.TerrainInfo;

        var unit = currNode.Unit;
        if (moveToNode.Unit != null)
        {
            GD.PrintErr("Tile Already has a unit on it");
            SelectedTile = null;
            CurrentMouseNode = null;
            return;
        }

        unit.TileNode = CurrentMouseNode;
        unit.Position = new Vector3(moveToNode.Position.X * 1.25f, moveToNode.TileHeight*1.25f, moveToNode.Position.Z * 1.25f);
        unit.PositionI = moveToNode.PositionI;
        currNode.Unit = null;
        moveToNode.Unit = unit;
        SelectedTile = null;
        if (!startGame.UnassignedUnits())
        {
            startGame.confirm.Visible = true;
        }
    }
    private void MainGameInputHandling()
    {
        if (Input.IsActionJustPressed("ui_click"))
        {
            HandleMainLeftClick();
        }
    }
    
    private void HandleMainLeftClick()
    {
        var nodeInfo = CurrentMouseNode?.TerrainInfo;
        if (nodeInfo == null) return;
        HandleUnitClick(nodeInfo, CurrentMouseNode);
    }
    private void HandleUnitClick(TerrainInfo UnitInfo, Tile UnitTile)
    {
        if (((SelectedTile == null && UnitInfo.Unit == null || SelectedTile == null && !UnitInfo.Unit.Data.IsMine()) || !Globals.GM.CurrentGameData.MyTurn)) // this is if the action is invalid
        {
            SelectedTile = null;
            return;
        }
        if (UnitInfo.Unit != null && UnitInfo.Unit.Data.IsMine() && SelectedTile == null)
        {
            SelectedTile = UnitTile;
            mainGame.UI.unitPanel.Select(SelectedTile.TerrainInfo.Unit);
            return;
        }
        if (UnitInfo.Unit == null) // the selected tile is our unit, the new tile doesn't have a unit on it, so we can move to it
        {
            SelectedTile.TerrainInfo.Unit.TileNode = SelectedTile;
            HandleMovement(SelectedTile.TerrainInfo, UnitInfo, SelectedTile.TerrainInfo.Unit);
        }
        else // the selected tile is our unit, the new tile has a unit on it, so we want to attack or support that unit
        {
            // Handle attack/ healing
            if (UnitInfo.Unit.Data.OwnerId != Globals.GM.clientId)
            {
                HandleAttack(SelectedTile.TerrainInfo.Unit, UnitInfo.Unit);
            }
            else
            {
                HandleSupport(SelectedTile.TerrainInfo.Unit, UnitInfo.Unit);
            }
        }

        SelectedTile = null;
    }
    private void HandleMovement(TerrainInfo fromPos, TerrainInfo toPos, IUnit unit)
    {
        unit.Move(fromPos, toPos);
    }
    private void HandleAttack(IUnit me, IUnit enemy)
    {
        if (mainGame.UI.unitPanel.SelectedSkill == null || mainGame.UI.unitPanel.SelectedSkill.Skill is not IAttack)
        {
            return;
        }
        ((IAttack)mainGame.UI.unitPanel.SelectedSkill.Skill).Attack(enemy);
    }

    private void HandleSupport(IUnit me, IUnit ally)
    {
        if (mainGame.UI.unitPanel.SelectedSkill == null || mainGame.UI.unitPanel.SelectedSkill.Skill is not ISupport)
        {
            return;
        }
        ((ISupport)mainGame.UI.unitPanel.SelectedSkill.Skill).Support(ally);
    }
}
