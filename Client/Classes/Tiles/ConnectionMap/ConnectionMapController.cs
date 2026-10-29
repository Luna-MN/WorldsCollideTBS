using System;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Packets.Classes.Tiles;
[Tool]
[GlobalClass]
public partial class ConnectionMapController : Resource
{
        [Export]
    public Dictionary<int, ConnectionMap> TileMaps {
        get;
        set;
    } = new();
    public ConnectionMap this[TileUtil.TileConnection i] => TileMaps[(int)i];
    public ConnectionInfo this[TileUtil.TileConnection i, TileUtil.ConnectionState j] => this[i][j];
    [ExportToolButton("GDBackup")] private Callable CallableGDBackup => new Callable(this, nameof(GDBackup));
    public void GDBackup()
    {
        foreach (var map in TileMaps.Values)
        {
            map.Initialize();
        }
    }
    public override Array<Dictionary> _GetPropertyList()
    {
        Array<Dictionary> list = new();
        for (int i = 0; i < TileMaps.Count; i++)
        {
            list.Add(new Dictionary()
            {
                {"name", ((TileUtil.TileConnection)TileMaps.Keys.ElementAt(i)).ToString()},
                {"type", (int)Variant.Type.Object},
                { "class_name", new StringName("ConnectionMap")},
                {"hint", (int)PropertyHint.ResourceType},
                {"hint_string", "ConnectionMap"}
            });
        }
        
        if (TileMaps.Count < Enum.GetValuesAsUnderlyingType<TileUtil.TileConnection>().Length)
        {
            list.Add(new Dictionary()
            {
                { "name", "New Value" },
                { "type", (int)Variant.Type.Object },
                { "class_name", new StringName("ConnectionMap") },
                { "hint", (int)PropertyHint.ResourceType },
                { "hint_string", "ConnectionMap" },
            });
        }
        
        return list;
    }
    public override Variant _Get(StringName property)
    {
        if (Enum.TryParse<TileUtil.TileConnection>(property, out var tile))
        {
            if (TileMaps.TryGetValue((int)tile, out var info))
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
            while (TileMaps.ContainsKey(i))
            {
                i++;
                if (i >= Enum.GetValuesAsUnderlyingType<TileUtil.TileConnection>().Length)
                {
                    return false;
                }
            }

            TileMaps[i] = new ConnectionMap();
            NotifyPropertyListChanged();
        }
        if (Enum.TryParse<TileUtil.TileType>(property.ToString(), out var name))
        {
            if (value.Obj == null)
            {
                GD.Print("null haha");
                TileMaps.Remove((int)name);
                NotifyPropertyListChanged();
                return true;
            }
            TileMaps[(int)name] = (ConnectionMap)((ConnectionMap)value).Duplicate();
            TileMaps[(int)name].ConnectionMapController = this;
            NotifyPropertyListChanged();
            return true;
        }
        return false;
    }
}