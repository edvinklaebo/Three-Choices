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
        _servicesInstaller.Context?.UI.SetBindings(combatView.BuildBindings(result.Player, result.Enemy));
    }

    public void Hide()
    {
        _servicesInstaller?.CombatView?.Hide();
    }
}
