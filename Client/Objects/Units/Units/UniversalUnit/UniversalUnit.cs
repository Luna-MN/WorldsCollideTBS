using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public partial class UniversalUnit : Node3D, IUnit
{
    private const string MeshFolderPath = "res://Objects/Units/Mesh";
    
    public UnitData Data { get; set; }
    public Node3D TileNode { get; set; }
    [Export] public PackedScene MeshScene, DefaultMeshScene;
    public DefaultUnit Mesh;
    public Color Color;
    public List<IAttack> Attacks { get; set; }
    public List<ISupport> Supports { get; set; }
    public IMovement Movement { get; set; }
    public Vector2I PositionI { get; set; }

    public override void _Ready()
    {
        InitUnit();
    }
    public virtual void InitUnit()
    {
        if (Data == null)
        {
            GD.PrintErr("Unit data is null!");
            return;
        }

        if (MeshScene == null)
        {
            FindMesh(Data.UnitName);
        }

        Mesh = MeshScene?.Instantiate<DefaultUnit>();
        BuildNamePlate();
        Mesh?.ChangeColor(Color);
        AddChild(Mesh);
        
        
        FindAttacks(Data.Attacks);
        FindMovement(Data.Movement);
        FindSupports(Data.Support);
        Movement = new DefaultMovement();
        Movement.InitMovement(this, this);
        Attacks?.ForEach(a => a.InitAttack(this, this));

    }
    private void BuildNamePlate()
    {
        Mesh.NamePlate.Text = Data.UnitName;
    }
    private void FindMesh(string meshName)
    {
        if (string.IsNullOrWhiteSpace(meshName))
        {
            GD.PrintErr("Mesh name is empty.");
            MeshScene = DefaultMeshScene;
            return;
        }

        string scenePath = FindScenePathRecursive(MeshFolderPath, meshName);

        if (string.IsNullOrEmpty(scenePath))
        {
            GD.PrintErr($"Mesh scene '{meshName}' was not found in '{MeshFolderPath}'.");
            MeshScene = DefaultMeshScene;
            return;
        }

        MeshScene = ResourceLoader.Load<PackedScene>(scenePath);

        if (MeshScene == null)
        {
            GD.PrintErr($"Failed to load mesh scene: {scenePath}");
        }
    }
    
    private void FindAttacks(List<string> attacks)
    {
        if (attacks == null || attacks.Count <= 0 || (attacks.Count == 1 && attacks[0] == ""))
        {
            return;
        }
        Attacks = new();
        foreach (var attackName in attacks)
        {
            var attackType = Assembly.GetExecutingAssembly().GetTypes().Where(t =>
                typeof(IAttack).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && t.Name == attackName).ToList()[0];
            var attack = (IAttack)Activator.CreateInstance(attackType);
            Attacks.Add(attack);
        }
    }

    private void FindMovement(string movementName)
    {
        var movementType = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(IMovement).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && t.Name == movementName).ToList()[0];
        Movement = (IMovement)Activator.CreateInstance(movementType);
    }
    
    private void FindSupports(List<string> supports)
    {
        if (supports == null || supports.Count <= 0 || (supports.Count == 1 && supports[0] == ""))
        {
            return;
        }
        Supports = new();
        foreach (var supportName in supports)
        {
            var supportType = Assembly.GetExecutingAssembly().GetTypes().Where(t =>
                typeof(ISupport).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && t.Name == supportName).ToList()[0];
            var support = (ISupport)Activator.CreateInstance(supportType);
            Supports.Add(support);
        }
    }
    public virtual void Move(TerrainInfo fromPos, TerrainInfo toPos, bool message = false)
    {
        var path = PathFinding.FindCheapestPath(fromPos, toPos);
        Movement.Move(path, message);
    }
    public virtual void Attack(IUnit unit)
    {
    }
    public void Support(IUnit unit)
    {
    }
    
    private string FindScenePathRecursive(string folderPath, string meshName)
    {
        using DirAccess dir = DirAccess.Open(folderPath);

        if (dir == null)
        {
            GD.PrintErr($"Could not open directory: {folderPath}");
            return null;
        }

        foreach (string fileName in dir.GetFiles())
        {
            string extension = fileName.GetExtension();

            if (extension != "tscn" && extension != "scn")
                continue;

            string fileBaseName = fileName.GetBaseName();

            if (!string.Equals(fileBaseName, meshName, StringComparison.OrdinalIgnoreCase))
                continue;

            return $"{folderPath}/{fileName}";
        }

        foreach (string directoryName in dir.GetDirectories())
        {
            if (directoryName.StartsWith("."))
                continue;

            string result = FindScenePathRecursive($"{folderPath}/{directoryName}", meshName);

            if (!string.IsNullOrEmpty(result))
                return result;
        }
        return null;
    }
}