using Godot;
using System;
using Packets;

public partial class SkillButton : Button
{
    public SkillType Type;
    public ISkill Skill;
    public override void _Ready()
    {
        Text = Skill.Name();
    }
}
