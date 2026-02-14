using System;

public static class MonsterName
{
    public static readonly string[] prefixes =
    {
        "Gor", "Zal", "Thar", "Vor", "Kri", "Mok", "Fen", "Drak"
    };

    public static readonly string[] suffixes =
    {
        "gore", "fang", "tail", "claw", "maw", "spike", "horn", "shade"
    };

    public static readonly string[] titles =
    {
        "the Terrible", "the Shadow", "the Mighty", "the Cruel", "the Ancient"
    };

    public static Random random = new();

    /// <summary>
    ///     Generates a random monster name.
    /// </summary>
    public static string Random()
    {
        var prefix = prefixes[random.Next(prefixes.Length)];
        var suffix = suffixes[random.Next(suffixes.Length)];

        // 50% chance to add a title
        var title = "";
        if (random.NextDouble() < 0.5) title = " " + titles[random.Next(titles.Length)];

        return prefix + suffix + title;
    }
}