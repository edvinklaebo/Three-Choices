using UnityEngine;
using UnityEngine.SceneManagement;

public class RunController : MonoBehaviour
{
    [SerializeField] private VoidEventChannel requestNextFight;
    [SerializeField] private VoidEventChannel playerDiedEvent;
    
    public Unit Player;

    private int _fightIndex = 1;

    public RunState CurrentRun { get; private set; }
    public RunController Instance { get; private set; }

    public void Awake()
    {
        DontDestroyOnLoad(this);
        Instance = this;
    }

    private void OnEnable()
    {
        requestNextFight.OnRaised += HandleNextFight;
        playerDiedEvent.OnRaised += OnPlayerDied;
    }

    private void OnDisable()
    {
        requestNextFight.OnRaised -= HandleNextFight;
        playerDiedEvent.OnRaised -= OnPlayerDied;
    }

    public void ContinueRun()
    {
        SceneManager.LoadScene("DraftScene");
        CurrentRun = SaveService.Load();

        _fightIndex = CurrentRun.fightIndex;
        Player = CurrentRun.player;

        Player.Died += _ => playerDiedEvent.Raise();
        SaveService.Save(CurrentRun);
        requestNextFight.Raise();
    }

    public void StartNewRun(CharacterDefinition character)
    {
        if (character == null)
        {
            Debug.LogError("[Run] Cannot start run with null character");
            return;
        }

        Debug.Log($"[Run] Starting run with {character.DisplayName}");
        SceneManager.LoadScene("DraftScene");
        Player = CreatePlayerFromCharacter(character);
        Player.Died += _ => playerDiedEvent.Raise();

        CurrentRun = new RunState
        {
            fightIndex = _fightIndex,
            player = Player
        };

        SaveService.Save(CurrentRun);
        requestNextFight.Raise();
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