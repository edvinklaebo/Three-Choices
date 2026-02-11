using System;
using System.Collections.Generic;

[Serializable]
public class Stats
{
    public int MaxHP;
    public int CurrentHP;
    public int AttackPower;
    public int Armor;
    public int Speed;
    
    public IEnumerable<(string name, int value)> Enumerate()
    {
        yield return ("Max HP", MaxHP);
        yield return ("Current HP", CurrentHP);
        yield return ("Attack Power", AttackPower);
        yield return ("Armor", Armor);
        yield return ("Speed", Speed);
    }
}