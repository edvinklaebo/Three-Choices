using System.Collections;
using UnityEngine;

/// <summary>
/// Presentation layer: receives a combat result, feeds it to the animation runner, and signals
/// when the full visual sequence has completed.
/// Owns only the presentation sequencing timeline.
/// </summary>
public class CombatPresentationCoordinator : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private CombatReadyEventChannel _combatReady;
    [SerializeField] private CombatPresentationCompleteEventChannel _presentationComplete;

    [Header("References")]
    [SerializeField] private CombatAnimationRunner _animationRunner;
    [SerializeField] private CombatViewPresenter _viewPresenter;

    private void Awake()
    {
        if (_animationRunner == null)
            Log.Error("CombatPresentationCoordinator: _animationRunner is not assigned.");
        if (_viewPresenter == null)
            Log.Error("CombatPresentationCoordinator: _viewPresenter is not assigned.");
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

        _viewPresenter.Show(result);

        _animationRunner.EnqueueRange(result.Actions);
        _animationRunner.PlayAll(_viewPresenter.Context);

        yield return _animationRunner.WaitForCompletion();

        _viewPresenter.Hide();

        _presentationComplete?.Raise(result.Player);
    }
}
