using Godot;
using System;
using System.Linq;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class TopTileMapController : Resource
{
    [Export]
    public Dictionary<int, TopTileMap> TileMaps {
        get;
        set;
    } = new();
    public TopTileMap this[TileUtil.TileTopType j] => TileMaps[(int)j];
    public TopTileInfo this[TileUtil.TileTopType j, TileUtil.TileTopState i] => this[j][i];
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
                {"name", ((TileUtil.TileTopType)TileMaps.Keys.ElementAt(i)).ToString()},
                {"type", (int)Variant.Type.Object},
                { "class_name", new StringName("TileMap")},
                {"hint", (int)PropertyHint.ResourceType},
                {"hint_string", "TileMap"}
            });
        }
        
        if (TileMaps.Count < Enum.GetValuesAsUnderlyingType<TileUtil.TileTopType>().Length)
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
        if (Enum.TryParse<TileUtil.TileTopType>(property, out var tile))
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
                if (i >= Enum.GetValuesAsUnderlyingType<TileUtil.TileTopType>().Length)
                {
                    return false;
                }
            }

            TileMaps[i] = new TopTileMap();
            NotifyPropertyListChanged();
        }
        if (Enum.TryParse<TileUtil.TileTopType>(property.ToString(), out var name))
        {
            if (value.Obj == null)
            {
                GD.Print("null haha");
                TileMaps.Remove((int)name);
                NotifyPropertyListChanged();
                return true;
            }
            TileMaps[(int)name] = (TopTileMap)((TopTileMap)value).Duplicate();
            TileMaps[(int)name].TileMapController = this;
            NotifyPropertyListChanged();
            return true;
        }
        return false;
    }
}
