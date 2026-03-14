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
        private const string GameScene = "GameScene";

        [Header("Events")]
        [SerializeField] private VoidEventChannel requestNextFight;
        [SerializeField] private VoidEventChannel playerDiedEvent;
        [SerializeField] private VoidEventChannel combatEndedWithPlayerDeath;
        [SerializeField] private VoidEventChannel _continueRunRequested;
        [SerializeField] private FightStartedEventChannel _fightStarted;
        [SerializeField] private BossFightEventChannel _bossFightStarted;

        [Header("Boss")]
        [SerializeField] private BossPool bossPool;

        [Header("References")] public Unit Player;

        public RunState CurrentRun { get; private set; }

        private RunProgressionService _progressionService;

        public void Awake()
        {
            DontDestroyOnLoad(this);
            BossManager bossManager = bossPool != null ? new BossManager(bossPool) : null;
            _progressionService = new RunProgressionService(_fightStarted, bossManager, _bossFightStarted);
        }

        private void OnEnable()
        {
            requestNextFight.OnRaised += _progressionService.HandleNextFight;

            if (combatEndedWithPlayerDeath != null)
                combatEndedWithPlayerDeath.OnRaised += OnCombatEndedWithPlayerDeath;
            if (_continueRunRequested != null)
                _continueRunRequested.OnRaised += ContinueRun;

            GameEvents.CharacterSelected_Event += StartNewRun;
            GameEvents.ReturnToMainMenu_Event += HandleReturnToMainMenu;
        }

        private void OnDisable()
        {
            requestNextFight.OnRaised -= _progressionService.HandleNextFight;

            if (combatEndedWithPlayerDeath != null)
                combatEndedWithPlayerDeath.OnRaised -= OnCombatEndedWithPlayerDeath;
            if (_continueRunRequested != null)
                _continueRunRequested.OnRaised -= ContinueRun;

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

            PlayerInitializer.Initialize(Player, CurrentRun.player, playerDiedEvent);
            Player = CurrentRun.player;
            _progressionService.SetRun(CurrentRun, Player);

            SceneManager.LoadScene(GameScene);
        }

        private void StartNewRun(CharacterDefinition character)
        {
            if (character == null)
            {
                Log.Error("[Run] Cannot start run with null character");
                return;
            }

            var newPlayer = PlayerFactory.CreateFromCharacter(character);
            PlayerInitializer.Initialize(Player, newPlayer, playerDiedEvent);
            Player = newPlayer;

            CurrentRun = new RunState
            {
                fightIndex = 0,
                player = Player
            };
            _progressionService.SetRun(CurrentRun, Player);

            SaveService.Save(CurrentRun);
            SceneManager.LoadScene(GameScene);
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