using Godot;
using System;
using Util;

public partial class InputHandler : Node3D
{
    private Vector3 MousePosition;
    public Tile CurrentMouseNode;
    [Export] private bool StoreNode, MainGame;
    private Tile SelectedTile;
    public override void _Ready()
    {
        RayCast.Set(GetTree(), GetViewport(), GetWorld3D());
    }

    public override void _Process(double delta)
    {
        MousePos();
        MouseHover();
        if (MainGame) MainGameInputHandling();
    }
    private void MousePos()
    {
        MousePosition = RayCast.CastPosition();
    }
    private void MouseHover()
    {
        var obj = RayCast.CastObject()?.GetParent<Tile>();
        
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
    private void MainGameInputHandling()
    {
        if (Input.IsActionJustPressed("ui_click"))
        {
            HandleLeftClick();
        }
    }
    private void HandleLeftClick()
    {
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
