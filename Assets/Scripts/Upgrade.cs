using System;

public enum UpgradeType
{
    Stat,
    Ability,
    Passive
}

[Serializable]
public class Upgrade
{
    public string Name;
    public UpgradeType Type;
    public Action<Unit> Apply;
}