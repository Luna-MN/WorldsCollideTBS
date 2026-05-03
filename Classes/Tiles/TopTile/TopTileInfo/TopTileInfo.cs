using System;
using Godot;
[Tool]
[GlobalClass]
public partial class TopTileInfo : Resource
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
    public TileUtil.TileTopState TileTopType
    {
        get => _tileTopType;
        set => setTileType(value);
    }
    private TileUtil.TileTopState _tileTopType;
    private void setTileType(TileUtil.TileTopState state)
    {
        if (_parent != null && _parent.TileInfo != null)
        {
            if (_parent.TileInfo.ContainsKey((int)state))
            {
                return;
            }
            _parent.TileInfo.Remove((int)_tileTopType);
            _parent.TileInfo[(int)state] = this;
            _parent.NotifyPropertyListChanged();
        }
        _tileTopType = state;
    }
    private Resource _gdBackup;
    
    private TopTileMap _parent;

    public TopTileMap Parent
    {
        get => _parent;
        set => _parent = value;
    }
    public TopTileInfo(TileUtil.TileTopState tileTopType, TopTileMap parent)
    {
        TileTopType = tileTopType;
        
        this._parent = parent;
    }
    public TopTileInfo() {}    
    private GDBackup backup;
    private string templatePath = "res://Prefabs/TileMaps/Template/Template.tres";
    public void LoadFromGDBackup(string ResourceSavePath)
    {
        backup = new GDBackup(this, ResourceSavePath, templatePath);
        backup.LoadOrSave();
    }
    
}