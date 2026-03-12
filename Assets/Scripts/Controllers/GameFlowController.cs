using Core;
using Core.Boss;

using Events;

using UnityEngine;

using Utils;

namespace Controllers
{
    /// <summary>
    /// Decides what happens after combat: raises the next-fight or player-death event.
    /// On boss fights (detected via <see cref="BossFightEventChannel"/>), raises
    /// <see cref="_bossFightEnded"/> instead of <see cref="_fightEnded"/> so that
    /// <see cref="BossRewardController"/> can handle artifact rewards.
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private CombatPresentationCompleteEventChannel _presentationComplete;
        [SerializeField] private VoidEventChannel _fightEnded;
        [SerializeField] private VoidEventChannel _bossFightEnded;
        [SerializeField] private VoidEventChannel _combatEndedWithPlayerDeath;
        [SerializeField] private BossFightEventChannel _bossFightStarted;

        private bool _wasBossFight;

        private void Awake()
        {
            if (this._fightEnded == null)
                Log.Error("GameFlowController: _fightEnded is not assigned.");
            if (this._combatEndedWithPlayerDeath == null)
                Log.Warning("GameFlowController: _combatEndedWithPlayerDeath is not assigned.");
        }

        private void OnEnable()
        {
            if (this._presentationComplete != null)
                this._presentationComplete.OnRaised += HandlePresentationComplete;
            if (this._bossFightStarted != null)
                this._bossFightStarted.OnRaised += OnBossFightStarted;
        }

        private void OnDisable()
        {
            if (this._presentationComplete != null)
                this._presentationComplete.OnRaised -= HandlePresentationComplete;
            if (this._bossFightStarted != null)
                this._bossFightStarted.OnRaised -= OnBossFightStarted;
        }

        private void OnBossFightStarted(BossDefinition boss)
        {
            this._wasBossFight = true;
            Log.Info($"[GameFlowController] Boss fight started: '{boss.Id}'. Post-fight reward flow enabled.");
        }

        private void HandlePresentationComplete(Unit player)
        {
            if (player.Stats.CurrentHP <= 0)
            {
                if (this._combatEndedWithPlayerDeath != null)
                    this._combatEndedWithPlayerDeath.Raise();
                else
                    Log.Warning("GameFlowController: _combatEndedWithPlayerDeath is not assigned");

                this._wasBossFight = false;
                return;
            }

            if (this._wasBossFight)
            {
                this._wasBossFight = false;
                this._bossFightEnded?.Raise();
                return;
            }

            this._fightEnded?.Raise();
        }
    }
}
