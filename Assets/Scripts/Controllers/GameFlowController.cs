using UnityEngine;

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
        if (_fightEnded == null)
            Log.Error("GameFlowController: _fightEnded is not assigned.");
        if (_combatEndedWithPlayerDeath == null)
            Log.Warning("GameFlowController: _combatEndedWithPlayerDeath is not assigned.");
    }

    private void OnEnable()
    {
        if (_presentationComplete != null)
            _presentationComplete.OnRaised += HandlePresentationComplete;
        if (_bossFightStarted != null)
            _bossFightStarted.OnRaised += OnBossFightStarted;
    }

    private void OnDisable()
    {
        if (_presentationComplete != null)
            _presentationComplete.OnRaised -= HandlePresentationComplete;
        if (_bossFightStarted != null)
            _bossFightStarted.OnRaised -= OnBossFightStarted;
    }

    private void OnBossFightStarted(BossDefinition boss)
    {
        _wasBossFight = true;
        Log.Info($"[GameFlowController] Boss fight started: '{boss.Id}'. Post-fight reward flow enabled.");
    }

    private void HandlePresentationComplete(Unit player)
    {
        if (player.Stats.CurrentHP <= 0)
        {
            if (_combatEndedWithPlayerDeath != null)
                _combatEndedWithPlayerDeath.Raise();
            else
                Log.Warning("GameFlowController: _combatEndedWithPlayerDeath is not assigned");

            _wasBossFight = false;
            return;
        }

        if (_wasBossFight)
        {
            _wasBossFight = false;
            _bossFightEnded?.Raise();
            return;
        }

        _fightEnded?.Raise();
    }
}
