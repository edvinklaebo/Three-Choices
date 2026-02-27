using System;

[Flags]
public enum ArtifactTag
{
    None          = 0,
    Fireball      = 1 << 0,
    ArcaneMissiles = 1 << 1,
    DoubleStrike  = 1 << 2,
    Lifesteal     = 1 << 3,
    Poison        = 1 << 4,
    Bleed         = 1 << 5,
    Thorns        = 1 << 6,
    Rage          = 1 << 7
}
