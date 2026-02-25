using UnityEngine;

/// <summary>
/// Application layer: listens for fight start events, runs pure combat logic, and emits the result.
/// No animation, no UI.
/// </summary>
public class CombatOrchestrator : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private FightStartedEventChannel _fightStarted;
    [SerializeField] private CombatReadyEventChannel _combatReady;
    [SerializeField] private VoidEventChannel _requestNextFight;
    [SerializeField] private VoidEventChannel _fightEnded;

    [Header("References")]
    [SerializeField] private EnemyFactory _enemyFactory;
    [SerializeField] private CombatSystemService _combatSystem;

    private bool _isFighting;

    private void Awake()
    {
        if (_enemyFactory == null)
            Log.Error("CombatOrchestrator: _enemyFactory is not assigned.");
        if (_combatSystem == null)
            Log.Error("CombatOrchestrator: _combatSystem is not assigned.");
        if (_combatReady == null)
            Log.Error("CombatOrchestrator: _combatReady is not assigned.");
    }

    private void Start()
    {
        _requestNextFight?.Raise();
    }

    private void OnEnable()
    {
        if (_fightStarted != null)
            _fightStarted.OnRaised += HandleStartFight;
        if (_fightEnded != null)
            _fightEnded.OnRaised += OnFightEnded;
    }

    private void OnDisable()
    {
        if (_fightStarted != null)
            _fightStarted.OnRaised -= HandleStartFight;
        if (_fightEnded != null)
            _fightEnded.OnRaised -= OnFightEnded;
    }

    private void HandleStartFight(Unit player, int fightIndex)
    {
        if (_isFighting)
            return;

        _isFighting = true;

        var enemy = _enemyFactory.Create(fightIndex);
        
        CombatLogger.Instance.RegisterUnit(enemy);

        var actions = _combatSystem.RunFight(player, enemy);

        _combatReady?.Raise(new CombatResult(player, enemy, actions));
    }

    private void OnFightEnded()
    {
        _isFighting = false;
    }
}
