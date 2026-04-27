using Godot;
using System;
using System.Linq;
using Godot.Collections;
[Tool]
[GlobalClass]
public partial class TileMapController : Resource
{
    [Export]
    public Dictionary<int, TileMap> TileMaps {
        get;
        set;
    } = new();
    public TileMap this[TileUtil.TileType i] => TileMaps[(int)i];
    public TileInfo this[TileUtil.TileType i, TileUtil.TileState j] => this[i][j];
    [ExportToolButton("GDBackup")] private Callable CallableGDBackup => new Callable(this, nameof(GDBackup));
    private void GDBackup()
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
                {"name", ((TileUtil.TileType)TileMaps.Keys.ElementAt(i)).ToString()},
                {"type", (int)Variant.Type.Object},
                { "class_name", new StringName("TileMap")},
                {"hint", (int)PropertyHint.ResourceType},
                {"hint_string", "TileMap"}
            });
        }
        
        if (TileMaps.Count < Enum.GetValuesAsUnderlyingType<TileUtil.TileType>().Length)
        {
            list.Add(new Dictionary()
            {
                { "name", "New Value" },
                { "type", (int)Variant.Type.Object },
                { "class_name", new StringName("TileMap") },
                { "hint", (int)PropertyHint.ResourceType },
                { "hint_string", "TileMap" },
            });
        }
        
        return list;
    }
    public override Variant _Get(StringName property)
    {
        if (Enum.TryParse<TileUtil.TileType>(property, out var tile))
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
                if (i >= Enum.GetValuesAsUnderlyingType<TileUtil.TileType>().Length)
                {
                    return false;
                }
            }

            TileMaps[i] = new TileMap();
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
            TileMaps[(int)name] = (TileMap)((TileMap)value).Duplicate();
            TileMaps[(int)name].TileMapController = this;
            NotifyPropertyListChanged();
            return true;
        }
        return false;
    }
}
