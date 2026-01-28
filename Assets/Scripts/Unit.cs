using System;
using System.Collections.Generic;

[Serializable]
public class Unit
{
    public readonly string Name;
    public Stats Stats;

    public List<IAbility> Abilities = new();
    public List<IPassive> Passives = new();

    public Unit(string name)
    {
        Name = name;
    }
}