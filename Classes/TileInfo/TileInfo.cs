using Godot;
using System;
[Tool]
[GlobalClass]
public partial class TileInfo : Resource
{
    public PackedScene TileNode {
        get => _tileNode.Length == 1 ? _tileNode[0] : _tileNode[new Random().Next(0, _tileNode.Length)];
        set => setTileNode(value);
    }
    [Export]
    private PackedScene[] _tileNode;
    private void setTileNode(PackedScene scene)
    {
        var tiles = _tileNode;
        _tileNode = new PackedScene[tiles.Length + 1];
        tiles.CopyTo(_tileNode, 0);
        _tileNode[tiles.Length] = scene;
    }
    [Export]
    public TileUtil.TileState TileState
    {
        get => _tileState;
        set => setTileType(value);
    }
    private TileUtil.TileState _tileState;
    private void setTileType(TileUtil.TileState state)
    {
        if (_parent != null && _parent.TileInfo != null)
        {
            if (_parent.TileInfo.ContainsKey((int)state))
            {
                return;
            }
            _parent.TileInfo.Remove((int)_tileState);
            _parent.TileInfo[(int)state] = this;
            _parent.NotifyPropertyListChanged();
        }
        _tileState = state;
    }
    private Resource _gdBackup;
    
    private TileMap _parent;

    public TileMap Parent
    {
        get => _parent;
        set => _parent = value;
    }
    public TileInfo(TileUtil.TileState tileState, TileMap parent)
    {
        TileState = tileState;
        
        this._parent = parent;
    }
    public TileInfo() {}    
    private GDBackup backup;
    private string templatePath = "res://Prefabs/TileMaps/Template/Template.tres";
    public void LoadFromGDBackup(string ResourceSavePath)
    {
        backup = new GDBackup(this, ResourceSavePath, templatePath);
        backup.LoadOrSave();
    }
}
