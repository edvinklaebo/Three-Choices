using UnityEngine;
using UnityEngine.SceneManagement;

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

    private int _fightIndex;

    public RunState CurrentRun { get; private set; }

    public void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        requestNextFight.OnRaised += HandleNextFight;

        if (combatEndedWithPlayerDeath != null)
            combatEndedWithPlayerDeath.OnRaised += OnCombatEndedWithPlayerDeath;
        if (_continueRunRequested != null)
            _continueRunRequested.OnRaised += ContinueRun;

        GameEvents.CharacterSelected_Event += StartNewRun;
    }

    private void OnDisable()
    {
        requestNextFight.OnRaised -= HandleNextFight;

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

        // Use the loaded player directly - it's already been restored by SaveService.Load()
        InitializePlayer(CurrentRun.player);
        _fightIndex = CurrentRun.fightIndex;

        SceneManager.LoadScene(DraftScene);
    }

    private void StartNewRun(CharacterDefinition character)
    {
        if (character == null)
        {
            Log.Error("[Run] Cannot start run with null character");
            return;
        }

        var newPlayer = CreatePlayerFromCharacter(character);
        InitializePlayer(newPlayer);

        CurrentRun = new RunState
        {
            fightIndex = _fightIndex,
            player = Player
        };

        SaveService.Save(CurrentRun);
        SceneManager.LoadScene(DraftScene);
    }

    /// <summary>
    ///     Initializes the player and subscribes to necessary events.
    ///     This ensures consistent initialization whether loading or starting new.
    /// </summary>
    private void InitializePlayer(Unit player)
    {
        if (player == null)
        {
            Log.Error("[Run] Cannot initialize null player");
            return;
        }

        CombatLogger.Instance.UnregisterUnit(Player);
        Player = player;
        CombatLogger.Instance.RegisterUnit(Player);
        Player.Died += _ => playerDiedEvent.Raise();
    }

    private void HandleNextFight()
    {
        _fightIndex++;
        
        Save();
        
        _fightStarted?.Raise(Player, _fightIndex);
    }

    private void Save()
    {
        CurrentRun.fightIndex = _fightIndex;
        CurrentRun.player = Player;
        SaveService.Save(CurrentRun);
    }

    private static void OnCombatEndedWithPlayerDeath()
    {
        SaveService.Delete();
        SceneManager.LoadScene(GameOverScene);
    }

    private static Unit CreatePlayerFromCharacter(CharacterDefinition character)
    {
        return new Unit(character.DisplayName)
        {
            Stats = new Stats
            {
                Armor = character.Armor,
                AttackPower = character.Attack,
                CurrentHP = character.MaxHp,
                MaxHP = character.MaxHp,
                Speed = character.Speed
            }
        };
    }
}