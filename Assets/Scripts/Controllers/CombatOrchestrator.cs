using UnityEngine;

/// <summary>
/// Application layer: listens for fight start events, runs pure combat logic, and emits the result.
/// No animation, no UI.
/// On boss fights (<see cref="BossFightEventChannel"/> fired before <see cref="FightStartedEventChannel"/>),
/// a <see cref="Boss"/> unit is used as the enemy instead of the regular <see cref="EnemyFactory"/>.
/// </summary>
public class CombatOrchestrator : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private FightStartedEventChannel _fightStarted;
    [SerializeField] private CombatReadyEventChannel _combatReady;
    [SerializeField] private VoidEventChannel _requestNextFight;
    [SerializeField] private VoidEventChannel _fightEnded;
    [SerializeField] private BossFightEventChannel _bossFightStarted;

    [Header("References")]
    [SerializeField] private EnemyFactory _enemyFactory;
    [SerializeField] private CombatSystemService _combatSystem;

    private bool _isFighting;
    private BossDefinition _pendingBoss;

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
        if (_bossFightStarted != null)
            _bossFightStarted.OnRaised += OnBossFightStarted;
    }

    private void OnDisable()
    {
        if (_fightStarted != null)
            _fightStarted.OnRaised -= HandleStartFight;
        if (_fightEnded != null)
            _fightEnded.OnRaised -= OnFightEnded;
        if (_bossFightStarted != null)
            _bossFightStarted.OnRaised -= OnBossFightStarted;
    }

    private void OnBossFightStarted(BossDefinition boss)
    {
        _pendingBoss = boss;
    }

    private void HandleStartFight(Unit player, int fightIndex)
    {
        if (_isFighting)
            return;

        _isFighting = true;

        var enemy = CreateEnemy(fightIndex);

        CombatLogger.Instance.RegisterUnit(enemy);

        var actions = _combatSystem.RunFight(player, enemy);

        _combatReady?.Raise(new CombatResult(player, enemy, actions));
    }

    private Unit CreateEnemy(int fightIndex)
    {
        if (_pendingBoss != null)
        {
            var boss = new Boss(_pendingBoss);
            Log.Info("[CombatOrchestrator] Boss fight — using Boss unit", new
            {
                bossId = _pendingBoss.Id,
                fightIndex,
                maxHP = boss.Stats.MaxHP,
                attackPower = boss.Stats.AttackPower
            });
            _pendingBoss = null;
            return boss;
        }

        return _enemyFactory.Create(fightIndex);
    }

    private void OnFightEnded()
    {
        _isFighting = false;
    }
}
