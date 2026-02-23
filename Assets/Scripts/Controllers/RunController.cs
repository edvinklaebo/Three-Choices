using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///     Manages the run lifecycle: starting, continuing, and saving a run.
///     Delegates player creation to <see cref="PlayerFactory" />, player setup to
///     <see cref="PlayerInitializer" />, and fight progression to <see cref="RunProgressionService" />.
/// </summary>
public class RunController : MonoBehaviour
{
    private const string GameOverScene = "GameOver";
    private const string DraftScene = "DraftScene";

    [Header("Events")]
    [SerializeField] private VoidEventChannel requestNextFight;
    [SerializeField] private VoidEventChannel playerDiedEvent;
    [SerializeField] private VoidEventChannel combatEndedWithPlayerDeath;
    [SerializeField] private VoidEventChannel _continueRunRequested;
    [SerializeField] private FightStartedEventChannel _fightStarted;

    [Header("References")] public Unit Player;

    public RunState CurrentRun { get; private set; }

    private RunProgressionService _progressionService;

    public void Awake()
    {
        DontDestroyOnLoad(this);
        _progressionService = new RunProgressionService(_fightStarted);
    }

    private void OnEnable()
    {
        requestNextFight.OnRaised += _progressionService.HandleNextFight;

        if (combatEndedWithPlayerDeath != null)
            combatEndedWithPlayerDeath.OnRaised += OnCombatEndedWithPlayerDeath;
        if (_continueRunRequested != null)
            _continueRunRequested.OnRaised += ContinueRun;

        GameEvents.CharacterSelected_Event += StartNewRun;
    }

    private void OnDisable()
    {
        requestNextFight.OnRaised -= _progressionService.HandleNextFight;

        if (combatEndedWithPlayerDeath != null)
            combatEndedWithPlayerDeath.OnRaised -= OnCombatEndedWithPlayerDeath;
        if (_continueRunRequested != null)
            _continueRunRequested.OnRaised -= ContinueRun;

        GameEvents.CharacterSelected_Event -= StartNewRun;
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
        PlayerInitializer.Initialize(Player, newPlayer, playerDiedEvent);
        Player = newPlayer;

        CurrentRun = new RunState
        {
            fightIndex = 0,
            player = Player
        };
        _progressionService.SetRun(CurrentRun, Player);

        SaveService.Save(CurrentRun);
        SceneManager.LoadScene(DraftScene);
    }

    private static void OnCombatEndedWithPlayerDeath()
    {
        SaveService.Delete();
        SceneManager.LoadScene(GameOverScene);
    }
}