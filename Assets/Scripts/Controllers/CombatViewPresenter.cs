using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages combat view initialization, UI bindings, and visibility.
/// Owned by CombatPresentationCoordinator as its view collaborator.
/// </summary>
public class CombatViewPresenter : MonoBehaviour, ICombatViewPresenter
{
    [Header("Events")]
    [SerializeField] private VoidEventChannel _hideDraftUI;

    [Header("References")]
    [SerializeField] private CombatServicesInstaller _servicesInstaller;

    private void Awake()
    {
        if (_servicesInstaller == null)
            Log.Error("CombatViewPresenter: _servicesInstaller is not assigned. View presentation will not function correctly.");
    }

    public AnimationContext Context => _servicesInstaller?.Context;

    public void Show(CombatResult result)
    {
        _hideDraftUI?.Raise();

        var combatView = _servicesInstaller?.CombatView;
        if (combatView == null)
        {
            Log.Warning("CombatViewPresenter.Show: CombatView is null — view will not be displayed.");
            return;
        }

        combatView.Initialize(result.Player, result.Enemy);

        var ui = _servicesInstaller.Context?.UI;
        ui?.SetBindings(combatView.BuildBindings(result.Player, result.Enemy));

        if (ui != null)
        {
            ui.InitializeHealthDisplay(result.Player, GetInitialHP(result.Player, result.Actions), result.Player.Stats.MaxHP);
            ui.InitializeHealthDisplay(result.Enemy, GetInitialHP(result.Enemy, result.Actions), result.Enemy.Stats.MaxHP);
        }
    }

    private static int GetInitialHP(Unit unit, List<ICombatAction> actions)
    {
        foreach (var action in actions)
        {
            if (action is DamageAction damage && damage.Target == unit)
                return damage.TargetHPBefore;
        }

        // If the unit took no damage, post-combat HP equals pre-combat HP — no correction needed.
        return unit.Stats.CurrentHP;
    }

    public void Hide()
    {
        _servicesInstaller?.CombatView?.Hide();
    }
}
