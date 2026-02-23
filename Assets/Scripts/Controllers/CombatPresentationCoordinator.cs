using System.Collections;
using UnityEngine;

/// <summary>
/// Presentation layer: receives a combat result, feeds it to the animation runner, and signals
/// when the full visual sequence has completed.
/// </summary>
public class CombatPresentationCoordinator : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private CombatReadyEventChannel _combatReady;
    [SerializeField] private CombatPresentationCompleteEventChannel _presentationComplete;
    [SerializeField] private VoidEventChannel _hideDraftUI;

    [Header("References")]
    [SerializeField] private CombatAnimationRunner _animationRunner;
    [SerializeField] private CombatServicesInstaller _servicesInstaller;

    private void Awake()
    {
        if (_animationRunner == null)
            Log.Error("CombatPresentationCoordinator: _animationRunner is not assigned.");
        if (_servicesInstaller == null)
            Log.Error("CombatPresentationCoordinator: _servicesInstaller is not assigned.");
        if (_presentationComplete == null)
            Log.Error("CombatPresentationCoordinator: _presentationComplete is not assigned.");
    }

    private void OnEnable()
    {
        if (_combatReady != null)
            _combatReady.OnRaised += HandleCombatReady;
    }

    private void OnDisable()
    {
        if (_combatReady != null)
            _combatReady.OnRaised -= HandleCombatReady;
    }

    private void HandleCombatReady(CombatResult result)
    {
        StartCoroutine(PresentCombat(result));
    }

    private IEnumerator PresentCombat(CombatResult result)
    {
        // Prevent visual overlap if a previous animation is still playing
        if (_animationRunner.IsRunning)
            yield return _animationRunner.WaitForCompletion();

        _hideDraftUI?.Raise();

        var combatView = _servicesInstaller.CombatView;
        if (combatView)
        {
            combatView.Initialize(result.Player, result.Enemy);
            _servicesInstaller.Context?.UI.SetBindings(combatView.BuildBindings(result.Player, result.Enemy));
        }

        _animationRunner.EnqueueRange(result.Actions);
        _animationRunner.PlayAll(_servicesInstaller.Context);

        yield return _animationRunner.WaitForCompletion();

        if (combatView)
            combatView.Hide();

        _presentationComplete?.Raise(result.Player);
    }
}
