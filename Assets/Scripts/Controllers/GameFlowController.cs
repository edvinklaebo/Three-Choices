using UnityEngine;

/// <summary>
/// Decides what happens after combat: raises the next-fight or player-death event.
/// </summary>
public class GameFlowController : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private CombatPresentationCompleteEventChannel _presentationComplete;
    [SerializeField] private VoidEventChannel _fightEnded;
    [SerializeField] private VoidEventChannel _combatEndedWithPlayerDeath;

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
    }

    private void OnDisable()
    {
        if (_presentationComplete != null)
            _presentationComplete.OnRaised -= HandlePresentationComplete;
    }

    private void HandlePresentationComplete(Unit player)
    {
        if (player.Stats.CurrentHP <= 0)
        {
            if (_combatEndedWithPlayerDeath != null)
                _combatEndedWithPlayerDeath.Raise();
            else
                Log.Warning("GameFlowController: _combatEndedWithPlayerDeath is not assigned");
            return;
        }

        _fightEnded?.Raise();
    }
}
