using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager Instance { get; set; }

    [SerializeField] private DraftUI draftUI;
    [SerializeField] private BattleUI battleUI;
    
    private Unit _player;
    private Unit _enemy;

    private DraftSystem _draft;
    
    private int _fightIndex = 1;

    private void OnEnable()
    {
        _player.Died += OnPlayerDied;
    }

    private void OnDisable()
    {
        _player.Died -= OnPlayerDied;
    }

    
    private void Awake()
    {
        Log.Info($"Awake called on {nameof(GameManager)}");

        if (Instance != null && Instance != this)
        {
            Log.Warn($"Duplicate {nameof(GameManager)} detected. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        var upgradeRepo = ScriptableObject.CreateInstance<UpgradePool>();
        _draft = new DraftSystem(upgradeRepo);

        _player = new Unit("Edvin")
        {
            Stats = new Stats
            {
                Armor = 45,
                AttackPower = 7,
                CurrentHP = 100,
                MaxHP = 100,
                Speed = 10
            }
        };
        
        Log.Info("Player initialized", new
        {
            player = _player
        });

        StartNextFight();
    }

    private void StartNextFight()
    {
        _enemy = EnemyFactory.Create(_fightIndex);
        Log.Info($"Starting fight {_fightIndex}", new
        {
            playerHP = _player.Stats.CurrentHP,
            enemy = _enemy.Name,
            enemyHP = _enemy.Stats.CurrentHP
        });
        
        battleUI.Show($"{_player.Name} {_player.Stats.CurrentHP} vs {_enemy.Name} {_enemy.Stats.CurrentHP}");
        
        CombatSystem.RunFight(_player, _enemy);

        Log.Info($"Fight {_fightIndex} ended", new
        {
            playerHP = _player.Stats.CurrentHP,
            enemyHP = _enemy.Stats.CurrentHP
        });

        if (_player.Stats.CurrentHP <= 0)
        {
            Log.Info("Player defeated. Game over.");
            return;
        }

        ShowDraft();
    }

    private void ShowDraft()
    {
        var draft = _draft.GenerateDraft(3);
        
        Log.Info("Calling DraftUI.Show", new
        {
            draftUIInstanceId = draftUI ? draftUI.GetInstanceID() : -1,
            thisInstanceId = GetInstanceID()
        });

        draftUI.Show(draft, OnUpgradePicked);
    }

    private void OnUpgradePicked(UpgradeDefinition upgrade)
    {
        Log.Info("Upgrade picked", new
        {
            upgrade = upgrade.DisplayName,
            playerBefore = new
            {
                hp = _player.Stats.CurrentHP,
                armor = _player.Stats.Armor,
                ap = _player.Stats.AttackPower,
                speed = _player.Stats.Speed
            }
        });

        UpgradeApplier.Apply(upgrade, _player);

        Log.Info("Player updated after upgrade", new
        {
            playerAfter = new
            {
                hp = _player.Stats.CurrentHP,
                armor = _player.Stats.Armor,
                ap = _player.Stats.AttackPower,
                speed = _player.Stats.Speed
            }
        });

        _fightIndex++;

        StartCoroutine(StartNextFightDelayed(3f));
    }

    private IEnumerator StartNextFightDelayed(float delaySeconds)
    {
        Log.Info("Delaying next fight", new
        {
            delaySeconds,
            fightIndex = _fightIndex
        });

        yield return new WaitForSeconds(delaySeconds);

        Log.Info("Starting next fight after delay", new
        {
            fightIndex = _fightIndex
        });

        StartNextFight();
    }

    private void OnPlayerDied(Unit unit)
    {
        SceneManager.LoadScene("GameOver");
    }
}
