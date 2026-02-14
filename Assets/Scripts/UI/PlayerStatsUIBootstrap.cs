using UnityEngine;

public class PlayerStatsUIBootstrap : MonoBehaviour
{
    [SerializeField] private StatsPanelUI panel;
    [SerializeField] private RunController runController;
    [SerializeField] private VoidEventChannel requestNextFight;
    [SerializeField] private HealthBarUI _healthBar;


    public void Awake()
    {
        runController = FindFirstObjectByType<RunController>();
    }

    private void OnEnable()
    {
        requestNextFight.OnRaised += ShowStats;
        requestNextFight.OnRaised += InitHealthBar;
    }

    private void OnDisable()
    {
        requestNextFight.OnRaised -= ShowStats;
        requestNextFight.OnRaised -= InitHealthBar;
    }

    private void ShowStats()
    {
        panel.Show(runController.Player.Stats.ToViewData());
    }

    private void InitHealthBar()
    {
        if (runController?.Player == null)
        {
            Debug.LogWarning("PlayerStatsUIBootstrap: Cannot initialize health bar, player is not available yet");
            return;
        }
        if(!_healthBar.IsInitialized)
            _healthBar.Initialize(runController.Player);
    }
}