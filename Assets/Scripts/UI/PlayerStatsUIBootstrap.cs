using UnityEngine;

public class PlayerStatsUIBootstrap : MonoBehaviour
{
    [SerializeField] private StatsPanelUI panel;
    [SerializeField] private FightStartedEventChannel _fightStarted;
    [SerializeField] private HealthBarUI _healthBar;

    private void OnEnable()
    {
        if (_fightStarted != null) _fightStarted.OnRaised += OnFightStarted;
    }

    private void OnDisable()
    {
        if (_fightStarted != null) _fightStarted.OnRaised -= OnFightStarted;
    }

    private void OnFightStarted(Unit player, int fightIndex)
    {
        panel.Show(player.Stats.ToViewData());

        if (!_healthBar.IsInitialized)
            _healthBar.Initialize(player);
    }
}