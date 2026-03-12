using Core;
using Core.Boss;
using Core.Combat;

using Events;

using Systems;

using UnityEngine;

using Utils;

namespace Controllers
{
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
        [SerializeField] private VoidEventChannel _bossFightEnded;
        [SerializeField] private BossFightEventChannel _bossFightStarted;

        [Header("References")]
        [SerializeField] private EnemyFactory _enemyFactory;
        [SerializeField] private CombatSystemService _combatSystem;

        private bool _isFighting;
        private BossDefinition _pendingBoss;

        private void Awake()
        {
            if (this._enemyFactory == null)
                Log.Error("CombatOrchestrator: _enemyFactory is not assigned.");
            if (this._combatSystem == null)
                Log.Error("CombatOrchestrator: _combatSystem is not assigned.");
            if (this._combatReady == null)
                Log.Error("CombatOrchestrator: _combatReady is not assigned.");
        }

        private void Start()
        {
            this._requestNextFight?.Raise();
        }

        private void OnEnable()
        {
            if (this._fightStarted != null)
                this._fightStarted.OnRaised += HandleStartFight;
            if (this._fightEnded != null)
                this._fightEnded.OnRaised += OnFightEnded;
            if (this._bossFightEnded != null)
                this._bossFightEnded.OnRaised += OnFightEnded;
            if (this._bossFightStarted != null)
                this._bossFightStarted.OnRaised += OnBossFightStarted;
        }

        private void OnDisable()
        {
            if (this._fightStarted != null)
                this._fightStarted.OnRaised -= HandleStartFight;
            if (this._fightEnded != null)
                this._fightEnded.OnRaised -= OnFightEnded;
            if (this._bossFightEnded != null)
                this._bossFightEnded.OnRaised -= OnFightEnded;
            if (this._bossFightStarted != null)
                this._bossFightStarted.OnRaised -= OnBossFightStarted;
        }

        private void OnBossFightStarted(BossDefinition boss)
        {
            this._pendingBoss = boss;
        }

        private void HandleStartFight(Unit player, int fightIndex)
        {
            if (this._isFighting)
                return;

            this._isFighting = true;

            var enemy = CreateEnemy(fightIndex);

            CombatLogger.Instance.RegisterUnit(enemy);

            var actions = this._combatSystem.RunFight(player, enemy);

            this._combatReady?.Raise(new CombatResult(player, enemy, actions));
        }

        private Unit CreateEnemy(int fightIndex)
        {
            if (this._pendingBoss)
            {
                var boss = new Boss(this._pendingBoss);
                Log.Info("[CombatOrchestrator] Boss fight — using Boss unit", new
                {
                    bossId = this._pendingBoss.Id,
                    fightIndex,
                    maxHP = boss.Stats.MaxHP,
                    attackPower = boss.Stats.AttackPower
                });
                this._pendingBoss = null;
                return boss;
            }

            return this._enemyFactory.Create(fightIndex);
        }

        private void OnFightEnded()
        {
            this._isFighting = false;
        }
    }
}
