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
    public int UnitId { get; set; }
    public long JSONID { get; set; }
    public List<string> Skills { get; set; }
    public string Movement { get; set; }
    public long MaxAP { get; set; }
    public long AP { get; set; }
    public long HP { get; set; }
    public long MaxHP { get; set; }
    public Speeds Speed { get; set; }
    public UnitData() { }
    
    public UnitData(UnitDataJSON data, ulong id, int unitID)
    {
        UnitName = data.Name;
        OwnerId = id;
        JSONID = data.ID;
        UnitId = unitID;
        Skills = string.IsNullOrWhiteSpace(data.Skills)
            ? null
            : data.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        Skills?.ForEach(s => s = s.Replace(" ", "").Trim());
        Movement = data.Movement.Replace(" ", "").Trim();
        MaxAP = data.AP;
        AP = data.AP;
        HP = data.MaxHP;
        MaxHP = data.MaxHP;
        Speed = (Speeds)data.speed;
    }
    public bool IsMine()
    {
        return OwnerId == Globals.GM.clientId;
    }
}