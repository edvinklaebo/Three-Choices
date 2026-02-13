using UnityEngine;

public class PlayerStatsUIBootstrap : MonoBehaviour
{
    [SerializeField] private StatsPanelUI panel;
    [SerializeField] private RunController runController;
    [SerializeField] private UpgradeEventChannel upgradePicked;


    public void Start()
    {
        runController = FindFirstObjectByType<RunController>();
    }

    private void OnEnable()
    {
        upgradePicked.OnRaised += ShowStats;
    }

    private void OnDisable()
    {
        upgradePicked.OnRaised -= ShowStats;
    }

    private void ShowStats(UpgradeDefinition obj)
    {
        panel.Show(runController.Player.Stats.ToViewData());
    }
}