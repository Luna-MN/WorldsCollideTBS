using Godot;
using System;

public partial class UnitPanel : Panel
{
    [Export] private RichTextLabel NamePlate, AP, HP;
    [Export] private HBoxContainer Attacks, Supports;
    [Export] private PackedScene SkillButtonScene;
    public SkillButton SelectedSkill;
    public void Select(IUnit unit)
    {
        SelectedSkill = null;
        if (unit == null)
        {
            Visible = false;
            return;
        }
        Visible = true;
        NamePlate.Text = unit.Data.UnitName;
        AP.Text = $"AP: {unit.Data.AP} / {unit.Data.MaxAP}";
        HP.Text = $"HP: {unit.Data.HP} / {unit.Data.MaxHP}";
        foreach (var child in Attacks.GetChildren())
        {
            child.QueueFree();
        }   
        foreach (var child in Supports.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var skill in unit.Skills)
        {
            var SB = SkillButtonScene.Instantiate<SkillButton>();
            SB.Skill = unit.Skills.Find(s => s.Name == skill.Name);
            SB.Type = SkillButton.TypeEnum.Attack;
            SB.ButtonUp += () => SelectedSkill = SB;
            Attacks.AddChild(SB);
        }
    }
}
