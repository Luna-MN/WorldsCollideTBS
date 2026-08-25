using System;
using System.Collections.Generic;
using Godot;

namespace Packets.Classes;

public static class DictionaryExtention
{
    public static List<Tile> Highlight(this Dictionary<Vector2I, TerrainInfo> tiles, Func<Tile, bool> select)
    {
        var tilesHighlit = new List<Tile>();
        foreach (TerrainInfo terrainInfo in tiles.Values)
        {
            var t = terrainInfo.tile.Select(select);
            if (t != null) tilesHighlit.Add(terrainInfo.tile);
        }
        return tilesHighlit;
    }
    public static void Deselect(this Dictionary<Vector2I, TerrainInfo> tiles)
    {
        foreach (TerrainInfo terrainInfo in tiles.Values)
        {
            terrainInfo.tile.Deselect();
        }
    }
    public static void Deselect(this List<Tile> tiles) => tiles.ForEach(t => t.Deselect());
}