

using System.Collections.Generic;

public class UnitData
{
    public enum Speeds
    {
        Normal,
        Fast,
        Slow
    }
    public string UnitName { get; set; }
    public ulong OwnerId { get; set; }
    public ulong UnitId { get; set; }
    public List<string> Attacks { get; set; }
    public string Movement { get; set; }
    public int AP { get; set; }
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public Speeds Speed { get; set; }
}