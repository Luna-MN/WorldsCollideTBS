using System;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Packets.Classes.Tiles;
[Tool]
[GlobalClass]
public partial class ConnectionMap : Resource
{
    public ConnectionInfo this[int i] => ConnectionInfo[i];
    public ConnectionInfo this[TileUtil.ConnectionState i] => ConnectionInfo[(int)i];
    public ConnectionMapController ConnectionMapController;
    [Export]
    public Dictionary<int, ConnectionInfo> ConnectionInfo
    {
        get;
        set;
    } = new(); 
    public override Array<Dictionary> _GetPropertyList()
    {
        Array<Dictionary> list = new();
        for (int i = 0; i < ConnectionInfo.Count; i++)
        {
            list.Add(new Dictionary()
            {
                {"name", ((TileUtil.ConnectionState)ConnectionInfo.Keys.ElementAt(i)).ToString()},
                {"type", (int)Variant.Type.Object},
                { "class_name", new StringName("ConnectionInfo")},
                {"hint", (int)PropertyHint.ResourceType},
                {"hint_string", "ConnectionInfo"}
            });
        }
        
        if (ConnectionInfo.Count < Enum.GetValuesAsUnderlyingType<TileUtil.ConnectionState>().Length)
        {
            list.Add(new Dictionary()
            {
                { "name", "New Value" },
                { "type", (int)Variant.Type.Object },
                { "class_name", new StringName("ConnectionInfo") },
                { "hint", (int)PropertyHint.ResourceType },
                { "hint_string", "ConnectionInfo" },
            });
        }
        
        return list;
    }
    public override Variant _Get(StringName property)
    {
        if (Enum.TryParse<TileUtil.ConnectionState>(property, out var tile))
        {
            if (ConnectionInfo.TryGetValue((int)tile, out var info))
            {
                return info;
            }
        }
        return default;
    }

    public override bool _Set(StringName property, Variant value)
    {
        if (property.ToString() == "New Value")
        {
            int i = 0;
            while (ConnectionInfo.ContainsKey(i))
            {
                i++;
                if (i >= Enum.GetValuesAsUnderlyingType<TileUtil.ConnectionState>().Length)
                {
                    return false;
                }
            }

            ConnectionInfo[i] = new ConnectionInfo();
            ConnectionInfo[i].Parent = this;
            ConnectionInfo[i].TileState = (TileUtil.ConnectionState)i;
            NotifyPropertyListChanged();
        }
        if (Enum.TryParse<TileUtil.ConnectionState>(property.ToString(), out var name))
        {
            if (value.Obj == null)
            {
                GD.Print("null haha");
                ConnectionInfo.Remove((int)name);
                NotifyPropertyListChanged();
                return true;
            }
            ConnectionInfo[(int)name] = (ConnectionInfo)((ConnectionInfo)value).Duplicate();
            ConnectionInfo[(int)name].Parent = this;
            NotifyPropertyListChanged();
            return true;
        }
        return false;
    }
    [ExportToolButton("Load from GDB")]
    private Callable CallableLoadFromGDBackup => new Callable(this, nameof(Initialize));
    public void Initialize()
    {
        var path = "res://Prefabs/ConnectionMaps/" + ResourceName;
        var dir = DirAccess.Open(path);
        if (dir == null)
        {
            Error error = DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(path));
            if (error == Error.Ok)
            {
                GD.Print("Folder successfully created: " + path);
            }
            else
            {
                GD.PrintErr("Could not create folder (" + path + "): " + error);
            }
        }
        path += "/";
        foreach (var tile in ConnectionInfo) 
        {
            tile.Value.LoadFromGDBackup(path + ((TileUtil.TileState)tile.Key) + "_GDBackup.tres");
        }
    }
}