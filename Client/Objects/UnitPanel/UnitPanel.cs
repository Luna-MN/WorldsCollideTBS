using Godot;
using System;

public partial class UnitPanel : Panel
{
    [Export] private RichTextLabel NamePlate, AP, HP;
    [Export] private HBoxContainer Skills;
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
        if (unit.Skills is not { Count: > 0 })
        {
            return;
        }
        Visible = true;
        NamePlate.Text = unit.Data.UnitName;
        AP.Text = $"AP: {unit.Data.AP} / {unit.Data.MaxAP}";
        HP.Text = $"HP: {unit.Data.HP} / {unit.Data.MaxHP}";
        foreach (var child in Skills.GetChildren())
        {
            child.QueueFree();
        }   
        foreach (var skill in unit.Skills)
        {
            var SB = SkillButtonScene.Instantiate<SkillButton>();
            SB.Skill = unit.Skills[skill.Key];
            SB.Type = SB.Skill.Type();
            SB.ButtonUp += () => SelectedSkill = SB;
            Skills.AddChild(SB);
        }
    }
}
