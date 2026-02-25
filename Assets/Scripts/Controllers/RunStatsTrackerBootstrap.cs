using UnityEngine;

/// <summary>
/// MonoBehaviour bootstrap that owns a <see cref="RunStatsTracker"/> instance and wires it
/// to game events. Persists across scene loads via <see cref="Object.DontDestroyOnLoad"/>.
/// </summary>
public class RunStatsTrackerBootstrap : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private CombatReadyEventChannel _combatReady;
    [SerializeField] private VoidEventChannel _fightEnded;

    private RunStatsTracker _tracker;
    private Unit _currentPlayer;
    private Unit _currentEnemy;

    public RunStats CurrentStats => _tracker.Stats;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _tracker = new RunStatsTracker();

        if (_combatReady == null)
            Log.Warning("RunStatsTrackerBootstrap: _combatReady is not assigned.");
        if (_fightEnded == null)
            Log.Warning("RunStatsTrackerBootstrap: _fightEnded is not assigned.");
    }

    private void OnEnable()
    {
        if (_combatReady != null)
            _combatReady.OnRaised += OnCombatReady;
        if (_fightEnded != null)
            _fightEnded.OnRaised += OnFightEnded;

        GameEvents.CharacterSelected_Event += OnNewRunStarted;
    }

    private void OnDisable()
    {
        if (_combatReady != null)
            _combatReady.OnRaised -= OnCombatReady;
        if (_fightEnded != null)
            _fightEnded.OnRaised -= OnFightEnded;

        GameEvents.CharacterSelected_Event -= OnNewRunStarted;
    }

    private void OnNewRunStarted(CharacterDefinition _)
    {
        _tracker.UnregisterPlayer(_currentPlayer);
        _tracker.UnregisterEnemy(_currentEnemy);
        _currentPlayer = null;
        _currentEnemy = null;
        _tracker.Reset();
    }

    private void OnCombatReady(CombatResult result)
    {
        if (_currentPlayer != result.Player)
        {
            _tracker.UnregisterPlayer(_currentPlayer);
            _currentPlayer = result.Player;
            _tracker.RegisterPlayer(_currentPlayer);
        }

        _tracker.UnregisterEnemy(_currentEnemy);
        _currentEnemy = result.Enemy;
        _tracker.RegisterEnemy(_currentEnemy);
    }

    private void OnFightEnded()
    {
        _tracker.IncrementFightsCompleted();
        _tracker.UnregisterEnemy(_currentEnemy);
        _currentEnemy = null;
    }
}
