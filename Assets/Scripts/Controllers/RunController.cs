using UnityEngine;
using UnityEngine.SceneManagement;

public class RunController : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private VoidEventChannel requestNextFight;
    [SerializeField] private VoidEventChannel playerDiedEvent;
    [SerializeField] private VoidEventChannel combatEndedWithPlayerDeath;
    [SerializeField] private VoidEventChannel _continueRunRequested;
    [SerializeField] private FightStartedEventChannel _fightStarted;

    [Header("References")]
    public Unit Player;

    private int _fightIndex = 1;

    public RunState CurrentRun { get; private set; }

    public void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        requestNextFight.OnRaised += HandleNextFight;
        playerDiedEvent.OnRaised += OnPlayerDied;

        if (combatEndedWithPlayerDeath != null) 
            combatEndedWithPlayerDeath.OnRaised += OnCombatEndedWithPlayerDeath;
        if (_continueRunRequested != null) 
            _continueRunRequested.OnRaised += ContinueRun;

        GameEvents.CharacterSelected_Event += StartNewRun;
    }

    private void OnDisable()
    {
        requestNextFight.OnRaised -= HandleNextFight;
        playerDiedEvent.OnRaised -= OnPlayerDied;

        if (combatEndedWithPlayerDeath != null) 
            combatEndedWithPlayerDeath.OnRaised -= OnCombatEndedWithPlayerDeath;
        if (_continueRunRequested != null) 
            _continueRunRequested.OnRaised -= ContinueRun;

        GameEvents.CharacterSelected_Event -= StartNewRun;
    }

    private void ContinueRun()
    {
        Log.Info("[Run] Continue run requested");
        
        CurrentRun = SaveService.Load();
        
        if (CurrentRun?.player == null)
        {
            Log.Error("[Run] Failed to load saved run - save data is invalid or corrupted");
            return;
        }
        
        // Use the loaded player directly - it's already been restored by SaveService.Load()
        InitializePlayer(CurrentRun.player);
        _fightIndex = CurrentRun.fightIndex;

        Log.Info($"[Run] Continuing run at fight {_fightIndex} with player {Player.Name}");
        SceneManager.LoadScene("DraftScene");
        // Note: requestNextFight will be raised by CombatController.Start() when DraftScene loads
    }

    private void StartNewRun(CharacterDefinition character)
    {
        if (character == null)
        {
            Log.Error("[Run] Cannot start run with null character");
            return;
        }

        Log.Info($"[Run] Starting run with {character.DisplayName}");
        
        var newPlayer = CreatePlayerFromCharacter(character);
        InitializePlayer(newPlayer);
        
        CurrentRun = new RunState
        {
            fightIndex = _fightIndex,
            player = Player
        };

        SaveService.Save(CurrentRun);
        
        SceneManager.LoadScene("DraftScene");
        // Note: requestNextFight will be raised by CombatController.Start() when DraftScene loads
    }
    
    /// <summary>
    /// Initializes the player and subscribes to necessary events.
    /// This ensures consistent initialization whether loading or starting new.
    /// </summary>
    private void InitializePlayer(Unit player)
    {
        if (player == null)
        {
            Log.Error("[Run] Cannot initialize null player");
            return;
        }
        
        Player = player;
        Player.Died += _ => playerDiedEvent.Raise();
        
        Log.Info($"[Run] Player initialized: {Player.Name} (HP: {Player.Stats.CurrentHP}/{Player.Stats.MaxHP}, Abilities: {Player.Abilities.Count}, Passives: {Player.Passives.Count})");
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

    private static void OnPlayerDied()
    {
        // Player died event - for immediate notifications
        // Game over logic is now handled by OnCombatEndedWithPlayerDeath
        // to allow death animation to complete
        Log.Info("Player died - waiting for death animation to complete");
    }

    private static void OnCombatEndedWithPlayerDeath()
    {
        Log.Info("Combat ended with player death - loading game over scene");
        SaveService.Delete();
        SceneManager.LoadScene("GameOver");
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