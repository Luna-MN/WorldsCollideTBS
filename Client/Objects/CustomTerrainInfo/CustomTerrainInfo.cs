using Godot;
using System;
[Tool]
public partial class CustomTerrainInfo : Node3D
{
    [ExportToolButton("Generate Terrain")]
    private Callable GenerateTerrainButton => Callable.From(GenerateTerrain);
    [Export]
    private PackedScene TerrainScene;
    [Export]
    private TileMapController TileMaps;
    [Export]
    private TopTileMapController TopTileMaps;
    public TerrainInfo TerrainInfo;
    [Export]
    private int TileHeight;
    [Export]
    private TileUtil.TileType TileType;
    [Export]
    private TileUtil.TileTopState TopTileState;
    [Export]
    private TileUtil.TileTopType TileTopType;

    public Tile hexInstance;
    public override void _EnterTree()
    {
        GenerateTerrain();
    }

    public void GenerateTerrain()
    {
        TileMaps.GDBackup();
        TopTileMaps.GDBackup();

        if (hexInstance != null)
        {
            hexInstance.QueueFree();
            hexInstance = null;
        }
        
        TerrainInfo = new TerrainInfo()
        {
            TileHeight = TileHeight,
            TileType = TileType,
            TopTileState = TopTileState,
            TileTopType = TileTopType,
        };
        hexInstance = TerrainScene.Instantiate<Tile>();
        AddChild(hexInstance);
        hexInstance.Owner = GetTree().EditedSceneRoot;
        hexInstance.Position = TerrainInfo.Position;
        hexInstance.RotationDegrees = new Vector3(0, 90, 0);
        hexInstance.Set(TileMaps[TerrainInfo.TileType], TopTileMaps[TerrainInfo.TileTopType], TerrainInfo.Position.X, TerrainInfo.Position.Z, TerrainInfo);
        hexInstance.GenerateTile();
    }

    public override void _Ready()
    {
        
    }
}

