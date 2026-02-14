using UnityEngine;

public class PlayerStatsUIBootstrap : MonoBehaviour
{
    [SerializeField] private StatsPanelUI panel;
    [SerializeField] private RunController runController;
    [SerializeField] private UpgradeEventChannel upgradePicked;
    [SerializeField] private VoidEventChannel onRunStarted;
    [SerializeField] private HealthBarUI _healthBar;


    public void Start()
    {
        runController = FindFirstObjectByType<RunController>();
    }

    private void OnEnable()
    {
        upgradePicked.OnRaised += ShowStats;
        onRunStarted.OnRaised += InitHealthBar;
    }

    private void OnDisable()
    {
        upgradePicked.OnRaised -= ShowStats;
        onRunStarted.OnRaised -= InitHealthBar;
    }

    private void ShowStats(UpgradeDefinition obj)
    {
        panel.Show(runController.Player.Stats.ToViewData());
    }

    private void InitHealthBar()
    {
        _healthBar.Initialize(runController.Player);
    }
}