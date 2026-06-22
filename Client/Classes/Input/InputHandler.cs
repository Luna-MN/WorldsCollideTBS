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
    [Export] private bool StoreNode, MainGame, CamMove;
    [Export] private Camera Camera;
    [Export] private float camSpeed;
    
    [ExportGroup("StartGame")] 
    [Export(PropertyHint.GroupEnable)] private bool StartGame;
    [Export] private StartGame startGame;

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
        var obj = RayCast.CastObject<Node3D>()?.GetParent<Tile>();
        
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
            if (SelectedTile == null)
            {
                SelectedTile = CurrentMouseNode;
                return;
            }
            StartGameMovement();
        }

        GD.Print(startGame.seedSelected, SelectedTile != null);
    }
    private void StartGameMovement()
    {
        if (CurrentMouseNode == null || SelectedTile == null)
        {
            return;
        }
        var currNode = SelectedTile.TerrainInfo;
            
        var moveToNode = CurrentMouseNode.TerrainInfo;

        var unit = currNode.Unit;
        if (moveToNode.Unit != null)
        {
            GD.PrintErr("Tile Already has a unit on it");
            return;
        }
        
        unit.Position = new Vector3(moveToNode.Position.X * 1.25f, moveToNode.TileHeight + 1, moveToNode.Position.Z * 1.25f);
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
        GD.Print(CurrentMouseNode.Position.ToString());
        if (CurrentMouseNode == null) return;
        var nodeInfo = CurrentMouseNode.TerrainInfo;
        if (nodeInfo == null) return;
        HandleUnitClick(nodeInfo, CurrentMouseNode);
        // if (nodeInfo.Unit != null || SelectedTile != null)
        // {
        //     HandleUnitClick(nodeInfo, CurrentMouseNode);
        // }
    }
    private void HandleUnitClick(TerrainInfo UnitInfo, Tile UnitTile)
    {
        if (SelectedTile == null && UnitInfo.Unit == null)
        {
            return;
        }
        if (SelectedTile == null && UnitInfo.Unit != null)
        {
            SelectedTile = UnitTile;
            return;
        }
        if (SelectedTile != null && UnitInfo.Unit == null)
        {
            HandleMovement(SelectedTile.TerrainInfo, UnitInfo, SelectedTile.TerrainInfo.Unit);
            SelectedTile = null;
            return;
        }
        if (SelectedTile != null && UnitInfo.Unit != null)
        {
            GD.PrintErr("Tile Already has a unit on it");
            HandleAttack(SelectedTile.TerrainInfo.Unit, UnitInfo.Unit);
            // Handle attack/ healing
        }
    }
    private void HandleMovement(TerrainInfo fromPos, TerrainInfo toPos, IUnit unit)
    {
        unit.Move(fromPos, toPos);
    }
    private void HandleAttack(IUnit me, IUnit enemy)
    {
        me.Attack(enemy);
    }
}
