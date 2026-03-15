using System;

namespace UI.Stats
{
    public readonly struct StatViewData
    {
        public readonly string Name;
        public readonly int Value;

        public StatViewData(string name, int value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value;
        }
    }
}