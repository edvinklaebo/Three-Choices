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

        public IEnumerable<(string name, int value)> Enumerate()
        {
            yield return ("Max HP", this.MaxHP);
            yield return ("Current HP", this.CurrentHP);
            yield return ("Attack Power", this.AttackPower);
            yield return ("Armor", this.Armor);
            yield return ("Speed", this.Speed);
        }
    }
}