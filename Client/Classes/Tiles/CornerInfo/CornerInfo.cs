using System.Collections.Generic;
using Godot;

public class CornerInfo
{
    // Sum of the axial coords of the (up to) 3 hexes touching this corner - unique per corner
    public Vector2I Id;
    public Vector2 Position;
    // 1-3 tiles (fewer than 3 on the edge of the map)
    public List<TerrainInfo> Tiles = [];
    // conections
    public TileUtil.TileConnection terrainConnection = TileUtil.TileConnection.defaultConnection;
    public TileUtil.ConnectionState connectionState = TileUtil.ConnectionState.Connection1;
    
    [System.Flags]
    private enum Dir { None = 0, L = 1, R = 2, UL = 4, UR = 8, DL = 16, DR = 32 }

    private static Dir Classify(Vector2 d)
    {
        // "straight" if within ~26° of horizontal
        if (Mathf.Abs(d.Y) < Mathf.Abs(d.X) * 0.5f)
            return d.X < 0 ? Dir.L : Dir.R;
        if (d.Y < 0)
            return d.X < 0 ? Dir.UL : Dir.UR;
        return d.X < 0 ? Dir.DL : Dir.DR;
    }

    public TileUtil.ConnectionState GetConnectionState(List<CornerInfo> othercorners)
    {
        var mask = Dir.None;
        var count = 0;
        foreach (var corner in othercorners)
        {
            if (corner == null)
                continue;
            mask |= Classify(corner.Position - Position);
            count++;
        }

        if (count < 2)
            return TileUtil.ConnectionState.Connection1; // TODO: river end/source tile

        return mask switch
        {
            Dir.L  | Dir.UR => TileUtil.ConnectionState.Connection1,
            Dir.UR | Dir.DR => TileUtil.ConnectionState.Connection2,
            Dir.UL | Dir.DL => TileUtil.ConnectionState.Connection3,
            Dir.UL | Dir.R  => TileUtil.ConnectionState.Connection4,
            Dir.L  | Dir.DR => TileUtil.ConnectionState.Connection5,
            Dir.DL | Dir.R  => TileUtil.ConnectionState.Connection6,
            _ => TileUtil.ConnectionState.Connection1
        };
    }
}
