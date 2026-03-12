using System.Collections;

using Core.Combat;

using Events;

using Systems;

using UnityEngine;

using Utils;

namespace Controllers
{
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
        [SerializeField] private CombatViewPresenter _viewPresenter;
    
        private void Awake()
        {
            if (this._animationRunner == null)
                Log.Error("CombatPresentationCoordinator: _animationRunner is not assigned.");
            if (this._viewPresenter == null)
                Log.Error("CombatPresentationCoordinator: _viewPresenter is not assigned.");
            if (this._presentationComplete == null)
                Log.Error("CombatPresentationCoordinator: _presentationComplete is not assigned.");
        }

        private void OnEnable()
        {
            if (this._combatReady != null)
                this._combatReady.OnRaised += HandleCombatReady;
        }

        private void OnDisable()
        {
            if (this._combatReady != null)
                this._combatReady.OnRaised -= HandleCombatReady;
        }

        private void HandleCombatReady(CombatResult result)
        {
            StartCoroutine(PresentCombat(result));
        }

        private IEnumerator PresentCombat(CombatResult result)
        {
            // Prevent visual overlap if a previous animation is still playing
            if (this._animationRunner.IsRunning)
                yield return this._animationRunner.WaitForCompletion();

            this._viewPresenter.Show(result);
            this._animationRunner.EnqueueRange(result.Actions);
            this._animationRunner.PlayAll(this._viewPresenter.Context);

            yield return this._animationRunner.WaitForCompletion();

            this._viewPresenter.Hide();

            yield return this._animationRunner.WaitForCompletion();

            this._presentationComplete?.Raise(result.Player);
        }
    }
}
