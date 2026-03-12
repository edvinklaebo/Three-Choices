using Core;

using Events;

using UI.Stats;

using UnityEngine;

namespace UI
{
    public class PlayerStatsUIBootstrap : MonoBehaviour
    {
        [SerializeField] private StatsPanelUI panel;
        [SerializeField] private FightStartedEventChannel _fightStarted;
        [SerializeField] private HealthBarUI _healthBar;

        private void OnEnable()
        {
            if (this._fightStarted != null) 
                this._fightStarted.OnRaised += OnFightStarted;
        }

        private void OnDisable()
        {
            if (this._fightStarted != null) 
                this._fightStarted.OnRaised -= OnFightStarted;
        }

        private void OnFightStarted(Unit player, int fightIndex)
        {
            this.panel.Show(player.Stats.ToViewData());
            this._healthBar.Bind(player);
        }
    }
}