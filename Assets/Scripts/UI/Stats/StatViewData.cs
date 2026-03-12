namespace UI.Stats
{
    public readonly struct StatViewData
    {
        public readonly string Name;
        public readonly int Value;

        public StatViewData(string name, int value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}