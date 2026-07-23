using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Packets;

public partial class UniversalUnit : Node3D, IUnit
{
    private const string MeshFolderPath = "res://Objects/Units/Mesh";

    public UnitData Data { get; set; }
    public Node3D TileNode { get; set; }
    [Export] public PackedScene MeshScene, DefaultMeshScene;
    public DefaultUnit Mesh;
    public Color Color;
    public Dictionary<string, ISkill> Skills { get; set; }
    public IMovement Movement { get; set; }
    public Vector2I PositionI { get; set; }
    public Dictionary<int, ICombatAction> SkillBuffer { get; set; } = new(); // <skillId, ICombatAction>
    public Dictionary<int, InflictAction> Inflictions { get; set; } = new(); // <skillId, InflictAction>

    #region Init

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


        FindSkills(Data.Skills);
        FindMovement(Data.Movement);
        Movement = new DefaultMovement();
        Movement.InitMovement(this, this);
        Skills?.Values.ToList().ForEach(a => a.Init(this, this, null)); // skillData

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

    private void FindSkills(List<string> skills)
    {
        if (skills == null || skills.Count <= 0 || (skills.Count == 1 && skills[0] == ""))
        {
            return;
        }

        Skills = new();
        foreach (var skillName in skills)
        {
            var skillType = Assembly.GetExecutingAssembly().GetTypes().Where(t =>
                    typeof(ISkill).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && t.Name == skillName)
                .ToList()[0];
            var skill = (ISkill)Activator.CreateInstance(skillType);
            Skills.Add(skillName, skill);
        }
    }

    private void FindMovement(string movementName)
    {
        var movementType = Assembly.GetExecutingAssembly().GetTypes().Where(t =>
                typeof(IMovement).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && t.Name == movementName)
            .ToList()[0];
        Movement = (IMovement)Activator.CreateInstance(movementType);
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

    #endregion

    public void NewTurn()
    {
        // process inflictions
        foreach (var inflict in Inflictions)
        {
            inflict.Value.Action.Execute(new CombatContext(this, this), inflict.Key);
            inflict.Value.Turns--;
            if (inflict.Value.Turns <= 0)
            {
                Inflictions.Remove(inflict.Key);
            }
        }
    }

    public virtual void AddToSkillBuffer(int skillId, ICombatAction action)
    {
        SkillBuffer.Add(skillId, action);
    }

    public void RemoveFromSkillBuffer(int skillId)
    {
        SkillBuffer[skillId].Invert(skillId);
        SkillBuffer.Remove(skillId);
    }

    public virtual void Move(TerrainInfo fromPos, TerrainInfo toPos, bool message = false)
    {
        var path = PathFinding.FindCheapestPath(fromPos, toPos);
        Movement.Move(path, message);
    }
    public virtual void Skill(Vector2I position, string skillName)
    {
        var skill = Skills[skillName];
        skill.Use(position);
    }

    public void Inflict(int skillId, ICombatAction action, int Turns)
    {
        Inflictions.Add(skillId, new InflictAction() { Action = action, Turns = Turns });
    }

    public void RemoveInflict(int skillId)
    {
        throw new NotImplementedException();
    }

    public void Damage(float amount)
    {
        Data.HP -= (long)amount;
    }
    public void Heal(float amount)
    {
        Data.HP += (long)amount;
    }
}