using UnityEngine;
using UnityEngine.SceneManagement;

public class RunController : MonoBehaviour
{
    [SerializeField] private VoidEventChannel requestNextFight;
    [SerializeField] private VoidEventChannel playerDiedEvent;
    [SerializeField] private VoidEventChannel combatEndedWithPlayerDeath;

    public Unit Player;

    public int _fightIndex = 1;

    public RunState CurrentRun { get; private set; }

    public void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        requestNextFight.OnRaised += HandleNextFight;
        playerDiedEvent.OnRaised += OnPlayerDied;

        if (combatEndedWithPlayerDeath != null) combatEndedWithPlayerDeath.OnRaised += OnCombatEndedWithPlayerDeath;
    }

    private void OnDisable()
    {
        requestNextFight.OnRaised -= HandleNextFight;
        playerDiedEvent.OnRaised -= OnPlayerDied;

        if (combatEndedWithPlayerDeath != null) combatEndedWithPlayerDeath.OnRaised -= OnCombatEndedWithPlayerDeath;
    }

    public void ContinueRun()
    {
        CurrentRun = SaveService.Load();
        
        if (CurrentRun?.player == null)
        {
            Log.Error("Failed to load saved run");
            return;
        }
        
        // Use the loaded player directly - it's already been restored by SaveService.Load()
        Player = CurrentRun.player;
        _fightIndex = CurrentRun.fightIndex;

        Player.Died += _ => playerDiedEvent.Raise();
        SceneManager.LoadScene("DraftScene");
        // Note: requestNextFight will be raised by CombatController.Start() when DraftScene loads
    }

    public void StartNewRun(CharacterDefinition character)
    {
        if (character == null)
        {
            Log.Error("[Run] Cannot start run with null character");
            return;
        }

        Log.Info($"[Run] Starting run with {character.DisplayName}");
        SceneManager.LoadScene("DraftScene");
        Player = CreatePlayerFromCharacter(character);
        Player.Died += _ => playerDiedEvent.Raise();

        CurrentRun = new RunState
        {
            fightIndex = _fightIndex,
            player = Player
        };

        SaveService.Save(CurrentRun);
        // Note: requestNextFight will be raised by CombatController.Start() when DraftScene loads
    }

    private void HandleNextFight()
    {
        _fightIndex++;
        Save();
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