using System;
using System.Linq;
using System.Text.Json;
using Godot;
using Packets;

public class GameDataUpdater
{
    private string version;
    private string path = "res://Classes/GameData/";
    private const string VersionFileName = "version.txt";
    private const string GameDataFileName = "game_data.json";
    private const string DefaultVersion = "0.0.0";

    public GameDataUpdater()
    {
        GetOrCreateTXT();
        Globals.GDH = new GameDataHandler();
    }
    public string GetVersion() => version;
    public void UpdateVersion(string newVersion, GameDataMessage gameData)
    {
        version = newVersion;

        string filePath = path + VersionFileName;

        using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        file.StoreString(version);

        var factions = new FactionDataJSON[gameData.Factions.Count];
        for (int i = 0; i < gameData.Factions.Count; i++)
        {
            factions[i] = new FactionDataJSON(gameData.Factions[i]);
        }
        
        var armies = new ArmyDataJSON[gameData.Armies.Count];
        for (int i = 0; i < gameData.Armies.Count; i++)
        {
            armies[i] = new ArmyDataJSON(gameData.Armies[i]);
        }
        
        var units = new UnitDataJSON[gameData.Units.Count];
        for (int i = 0; i < gameData.Units.Count; i++)
        {
            units[i] = new UnitDataJSON(gameData.Units[i]);
        }

        GameDataJSON data = new GameDataJSON(factions, armies, units);
        Globals.GDH.Init(data);
        SerializeGameData(data);
    }
    
    private void SerializeGameData(GameDataJSON data)
    {
        string filePath = path + GameDataFileName;
        
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        };

        string json = JsonSerializer.Serialize(data, options);

        using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        file.StoreString(json);
    }

    public void DeserializeGameData()
    {
        string filePath = path + GameDataFileName;
        if (!FileAccess.FileExists(filePath)) return;
        using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        string json = file.GetAsText();
        GameDataJSON data = JsonSerializer.Deserialize<GameDataJSON>(json, new JsonSerializerOptions{IncludeFields = true});
        Globals.GDH.Init(data);
        GD.Print(Globals.GDH.GetFactions().Count);
    }

    private void GetOrCreateTXT()
    {
        DirAccess.MakeDirRecursiveAbsolute(path);

        string filePath = path + VersionFileName;

        if (!FileAccess.FileExists(filePath))
        {
            version = DefaultVersion;
            using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
            file.StoreString(version);

            return;
        }

        using FileAccess existingFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        version = existingFile.GetAsText().Trim();
        
    }
}