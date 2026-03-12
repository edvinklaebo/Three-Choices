using Characters;

using Core;
using Core.Boss;

using Events;

using Systems;

using UnityEngine;
using UnityEngine.SceneManagement;

using Utils;

namespace Controllers
{
    /// <summary>
    ///     Manages the run lifecycle: starting, continuing, and saving a run.
    ///     Delegates player creation to <see cref="PlayerFactory" />, player setup to
    ///     <see cref="PlayerInitializer" />, and fight progression to <see cref="RunProgressionService" />.
    /// </summary>
    public class RunController : MonoBehaviour
    {
        private const string GameOverScene = "GameOver";
        private const string DraftScene = "DraftScene";
        private const string MainMenuScene = "MainMenu";

        [Header("Events")]
        [SerializeField] private VoidEventChannel requestNextFight;
        [SerializeField] private VoidEventChannel playerDiedEvent;
        [SerializeField] private VoidEventChannel combatEndedWithPlayerDeath;
        [SerializeField] private VoidEventChannel _continueRunRequested;
        [SerializeField] private FightStartedEventChannel _fightStarted;
        [SerializeField] private BossFightEventChannel _bossFightStarted;

        [Header("Boss")]
        [SerializeField] private BossRegistry _bossRegistry;

        [Header("References")] public Unit Player;

        public RunState CurrentRun { get; private set; }

        private RunProgressionService _progressionService;

        public void Awake()
        {
            DontDestroyOnLoad(this);
            BossManager bossManager = this._bossRegistry != null ? new BossManager(this._bossRegistry) : null;
            this._progressionService = new RunProgressionService(this._fightStarted, bossManager, this._bossFightStarted);
        }

        private void OnEnable()
        {
            this.requestNextFight.OnRaised += this._progressionService.HandleNextFight;

            if (this.combatEndedWithPlayerDeath != null)
                this.combatEndedWithPlayerDeath.OnRaised += OnCombatEndedWithPlayerDeath;
            if (this._continueRunRequested != null)
                this._continueRunRequested.OnRaised += ContinueRun;

            GameEvents.CharacterSelected_Event += StartNewRun;
            GameEvents.ReturnToMainMenu_Event += HandleReturnToMainMenu;
        }

        private void OnDisable()
        {
            this.requestNextFight.OnRaised -= this._progressionService.HandleNextFight;

            if (this.combatEndedWithPlayerDeath != null)
                this.combatEndedWithPlayerDeath.OnRaised -= OnCombatEndedWithPlayerDeath;
            if (this._continueRunRequested != null)
                this._continueRunRequested.OnRaised -= ContinueRun;

            GameEvents.CharacterSelected_Event -= StartNewRun;
            GameEvents.ReturnToMainMenu_Event -= HandleReturnToMainMenu;
        }

        private void ContinueRun()
        {
            CurrentRun = SaveService.Load();

            if (CurrentRun?.player == null)
            {
                Log.Error("[Run] Failed to load saved run - save data is invalid or corrupted");
                return;
            }

            PlayerInitializer.Initialize(this.Player, CurrentRun.player, this.playerDiedEvent);
            this.Player = CurrentRun.player;
            this._progressionService.SetRun(CurrentRun, this.Player);

            SceneManager.LoadScene(DraftScene);
        }

        private void StartNewRun(CharacterDefinition character)
        {
            if (character == null)
            {
                Log.Error("[Run] Cannot start run with null character");
                return;
            }

            var newPlayer = PlayerFactory.CreateFromCharacter(character);
            PlayerInitializer.Initialize(this.Player, newPlayer, this.playerDiedEvent);
            this.Player = newPlayer;

            CurrentRun = new RunState
            {
                fightIndex = 0,
                player = this.Player
            };
            this._progressionService.SetRun(CurrentRun, this.Player);

            SaveService.Save(CurrentRun);
            SceneManager.LoadScene(DraftScene);
        }

        private static void OnCombatEndedWithPlayerDeath()
        {
            SaveService.Delete();
            SceneManager.LoadScene(GameOverScene);
        }

        private void HandleReturnToMainMenu()
        {
            GameEvents.ReturnToMainMenu_Event -= HandleReturnToMainMenu;
            Destroy(gameObject);
        }
    }
}