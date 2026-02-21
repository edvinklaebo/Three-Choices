/// <summary>
///     Deterministic binding between a Unit and its UI components.
///     Built once when combat starts to avoid repeated component lookups.
/// </summary>
public sealed class UnitUIBinding
{
    public UnitView UnitView { get; init; }
    public HealthBarUI HealthBar { get; init; }
    public UnitHUDPanel HUDPanel { get; init; }
}
