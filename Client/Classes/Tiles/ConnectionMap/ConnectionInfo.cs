using Godot;
using System;
using Packets.Classes.Tiles;

[Tool]
[GlobalClass]
public partial class ConnectionInfo : Resource
{
    public PackedScene TileNode {
        get => _tileNode.Length == 1 ? _tileNode[0] : _tileNode[new Random().Next(0, _tileNode.Length)];
        set => setTileNode(value);
    }
    [Export]
    private PackedScene[] _tileNode = [];
    private void setTileNode(PackedScene scene)
    {
        var tiles = _tileNode;
        _tileNode = new PackedScene[tiles.Length + 1];
        tiles.CopyTo(_tileNode, 0);
        _tileNode[tiles.Length] = scene;
    }
    [Export]
    public TileUtil.ConnectionState TileState
    {
        get => _tileState;
        set => setTileType(value);
    }
    private TileUtil.ConnectionState _tileState;
    private void setTileType(TileUtil.ConnectionState state)
    {
        if (_parent != null && _parent.ConnectionInfo != null)
        {
            if (_parent.ConnectionInfo.ContainsKey((int)state))
            {
                return;
            }
            _parent.ConnectionInfo.Remove((int)_tileState);
            _parent.ConnectionInfo[(int)state] = this;
            _parent.NotifyPropertyListChanged();
        }
        _tileState = state;
    }
    private Resource _gdBackup;
    
    private ConnectionMap _parent;

    public ConnectionMap Parent
    {
        get => _parent;
        set => _parent = value;
    }
    private GDBackup backup;
    private string templatePath = "res://Prefabs/TileMaps/Template/Template.tres";
    public void LoadFromGDBackup(string ResourceSavePath)
    {
        backup = new GDBackup(this, ResourceSavePath, templatePath);
        backup.LoadOrSave();
    }
}
