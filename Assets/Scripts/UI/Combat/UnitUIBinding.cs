/// <summary>
///     Deterministic binding between a Unit and its UI components.
///     Built once when combat starts to avoid repeated component lookups.
/// </summary>
public sealed class UnitUIBinding
{
    public UnitView UnitView { get;  }
    public HealthBarUI HealthBar { get;  }
    public UnitHUDPanel HUDPanel { get;  }
    
    public UnitUIBinding(
        UnitView unitView,
        HealthBarUI healthBar,
        UnitHUDPanel hudPanel)
    {
        UnitView = unitView;
        HealthBar = healthBar;
        HUDPanel = hudPanel;
    }
}
