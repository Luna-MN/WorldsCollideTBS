using Godot;
using System;

public partial class Log : RichTextLabel
{
    private void message(string msg, Color color)
    {
        AppendText($"[color={color.ToHtml(false)}]{msg}[/color] \n");
    }
    public void info(string msg) => message(msg, Colors.White);
    public void warning(string msg) => message(msg, Colors.Yellow);
    public void error(string msg) => message(msg, Colors.OrangeRed);
    public void success(string msg) => message(msg, Colors.Green);
    public void chat(string chatType, string sender, string msg) => message($"[color={Colors.CornflowerBlue.ToHtml(false)}][{chatType}] {sender}[/color]: [i]{msg}[/i]", Colors.White);

}
