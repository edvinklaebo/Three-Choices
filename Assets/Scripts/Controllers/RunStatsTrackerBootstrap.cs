using Characters;

using Core;
using Core.Combat;

using Events;

using Systems;

using UnityEngine;

using Utils;

namespace Controllers
{
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
        
            if (this._combatReady == null)
                Log.Warning("RunStatsTrackerBootstrap: _combatReady is not assigned.");
            if (this._fightEnded == null)
                Log.Warning("RunStatsTrackerBootstrap: _fightEnded is not assigned.");
        }

        private void OnEnable()
        {
            if (this._combatReady != null)
                this._combatReady.OnRaised += OnCombatReady;
            if (this._fightEnded != null)
                this._fightEnded.OnRaised += OnFightEnded;

            GameEvents.CharacterSelected_Event += OnNewRunStarted;
        }

        private void OnDisable()
        {
            if (this._combatReady != null)
                this._combatReady.OnRaised -= OnCombatReady;
            if (this._fightEnded != null)
                this._fightEnded.OnRaised -= OnFightEnded;

            GameEvents.CharacterSelected_Event -= OnNewRunStarted;
        }

        private void OnNewRunStarted(CharacterDefinition _)
        {
            Instance.UnregisterPlayer(this._currentPlayer);
            Instance.UnregisterEnemy(this._currentEnemy);
            this._currentPlayer = null;
            this._currentEnemy = null;
            Instance.Reset();
        }

        private void OnCombatReady(CombatResult result)
        {
            if (this._currentPlayer != result.Player)
            {
                Instance.UnregisterPlayer(this._currentPlayer);
                this._currentPlayer = result.Player;
                Instance.RegisterPlayer(this._currentPlayer);
            }

            Instance.UnregisterEnemy(this._currentEnemy);
            this._currentEnemy = result.Enemy;
            Instance.RegisterEnemy(this._currentEnemy);
        }

        private void OnFightEnded()
        {
            Instance.IncrementFightsCompleted();
            Instance.UnregisterEnemy(this._currentEnemy);
            this._currentEnemy = null;
        }
    }
}
