using UnityEngine;

/// <summary>
/// MonoBehaviour bootstrap that owns a <see cref="RunStatsTracker"/> instance and wires it
/// to game events. Persists across scene loads via <see cref="Object.DontDestroyOnLoad"/>.
/// </summary>
public class RunStatsTrackerBootstrap : MonoBehaviour
{
    public static RunStatsTracker Instance { get; private set; }

    [Header("Events")]
    [SerializeField] private CombatReadyEventChannel _combatReady;
    [SerializeField] private VoidEventChannel _fightEnded;

    private Unit _currentPlayer;
    private Unit _currentEnemy;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = new RunStatsTracker();
        
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
        Instance.UnregisterPlayer(_currentPlayer);
        Instance.UnregisterEnemy(_currentEnemy);
        _currentPlayer = null;
        _currentEnemy = null;
        Instance.Reset();
    }

    private void OnCombatReady(CombatResult result)
    {
        if (_currentPlayer != result.Player)
        {
            Instance.UnregisterPlayer(_currentPlayer);
            _currentPlayer = result.Player;
            Instance.RegisterPlayer(_currentPlayer);
        }

        Instance.UnregisterEnemy(_currentEnemy);
        _currentEnemy = result.Enemy;
        Instance.RegisterEnemy(_currentEnemy);
    }

    private void OnFightEnded()
    {
        Instance.IncrementFightsCompleted();
        Instance.UnregisterEnemy(_currentEnemy);
        _currentEnemy = null;
    }
}
