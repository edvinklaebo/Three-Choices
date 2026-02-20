/// <summary>
/// Defines the ordered phases of a single attack resolution.
/// Each phase allows listeners to observe or modify the combat context before the next phase executes.
/// </summary>
public enum CombatPhase
{
    PreAction,
    DamageCalculation,
    Mitigation,
    DamageApplication,
    Healing,
    ResourceGain,
    StatusApplication,
    PostResolve
}
