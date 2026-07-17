using Godot;
using System;

public partial class SkillButton : Button
{
    public enum TypeEnum
    {
        Support,
        Attack
    }
    public TypeEnum Type;
    public ISkill Skill;
    public override void _Ready()
    {
        Text = Skill.Name();
    }
}
