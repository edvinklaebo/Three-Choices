using System;
using System.Collections.Generic;

namespace Core
{
    [Serializable]
    public class Stats
    {
        public int MaxHP;
        public int CurrentHP;
        public int AttackPower;
        public int Armor;
        public int Speed;
        public int Mana;
        public int MagicScaling;
        public int HealingScaling;
        public int StatusScaling;

        public IEnumerable<(string name, int value)> Enumerate()
        {
            yield return ("Max HP", MaxHP);
            yield return ("Current HP", CurrentHP);
            yield return ("Attack Power", AttackPower);
            yield return ("Armor", Armor);
            yield return ("Speed", Speed);
            yield return ("Mana", Mana);
            yield return ("Magic Scaling", MagicScaling);
            yield return ("Healing Scaling", HealingScaling);
            yield return ("Status Scaling", StatusScaling);
        }
    }
}