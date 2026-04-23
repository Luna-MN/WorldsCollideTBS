using Godot;
using System;
using System.Linq;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class TileMap : Resource
{
    public TileInfo this[int i] => TileInfo[i];
    public TileInfo this[TileUtil.TileState i] => TileInfo[(int)i];
    public TileMapController TileMapController;
    public Dictionary<int, TileInfo> TileInfo
    {
        get;
        set;
    } = new();
    public override Array<Dictionary> _GetPropertyList()
    {
        Array<Dictionary> list = new();
        for (int i = 0; i < TileInfo.Count; i++)
        {
            list.Add(new Dictionary()
            {
                {"name", ((TileUtil.TileState)TileInfo.Keys.ElementAt(i)).ToString()},
                {"type", (int)Variant.Type.Object},
                { "class_name", new StringName("TileInfo")},
                {"hint", (int)PropertyHint.ResourceType},
                {"hint_string", "TileInfo"}
            });
        }
        
        if (TileInfo.Count < Enum.GetValuesAsUnderlyingType<TileUtil.TileState>().Length)
        {
            list.Add(new Dictionary()
            {
                { "name", "New Value" },
                { "type", (int)Variant.Type.Object },
                { "class_name", new StringName("TileInfo") },
                { "hint", (int)PropertyHint.ResourceType },
                { "hint_string", "TileInfo" },
            });
        }
        
        return list;
    }
    public override Variant _Get(StringName property)
    {
        if (Enum.TryParse<TileUtil.TileState>(property, out var tile))
        {
            if (TileInfo.TryGetValue((int)tile, out var info))
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
            while (TileInfo.ContainsKey(i))
            {
                i++;
                if (i >= Enum.GetValuesAsUnderlyingType<TileUtil.TileState>().Length)
                {
                    return false;
                }
            }

            TileInfo[i] = new TileInfo((TileUtil.TileState)i, this);
            NotifyPropertyListChanged();
        }
        if (Enum.TryParse<TileUtil.TileState>(property.ToString(), out var name))
        {
            if (value.Obj == null)
            {
                GD.Print("null haha");
                TileInfo.Remove((int)name);
                NotifyPropertyListChanged();
                return true;
            }
            TileInfo[(int)name] = (TileInfo)((TileInfo)value).Duplicate();
            TileInfo[(int)name].Parent = this;
            NotifyPropertyListChanged();
            return true;
        }
        return false;
    }
    [ExportToolButton("Load from GDB")]
    private Callable CallableLoadFromGDBackup => new Callable(this, nameof(Initialize));
    public void Initialize()
    {
        var path = "res://Prefabs/TileMaps/" + ResourceName;
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
        foreach (var tile in TileInfo) 
        {
            tile.Value.LoadFromGDBackup(path + ((TileUtil.TileState)tile.Key) + "_GDBackup.tres");
        }
    }
}
