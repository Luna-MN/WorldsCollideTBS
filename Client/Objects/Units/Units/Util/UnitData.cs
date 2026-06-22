

using System;
using System.Collections.Generic;
using System.Linq;

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
    public long UnitId { get; set; }
    public List<string> Attacks { get; set; }
    public string Movement { get; set; }
    public long AP { get; set; }
    public long HP { get; set; }
    public long MaxHP { get; set; }
    public Speeds Speed { get; set; }
    public UnitData() { }

    public UnitData(UnitDataJSON data, ulong id)
    {
        UnitName = data.Name;
        OwnerId = id;
        UnitId = data.ID;
        Attacks = string.IsNullOrWhiteSpace(data.Attacks)
            ? null
            : data.Attacks.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        Attacks?.ForEach(s => s = s.Replace(" ", "").Trim());
        Movement = data.Movement.Replace(" ", "").Trim();
        AP = data.AP;
        HP = data.MaxHP;
        MaxHP = data.MaxHP;
        Speed = (Speeds)data.speed;
    }
}