using Controllers;

using Core;
using Core.Boss;

using Events;

namespace Systems
{
    /// <summary>
    ///     Handles fight progression: tracks the current fight index, raises the
    ///     <see cref="FightStartedEventChannel" /> event (and <see cref="BossFightEventChannel" /> on boss
    ///     fights), and persists run state after each fight.
    ///     Boss fights occur every <see cref="BossManager.BossFightInterval" /> fights.
    /// </summary>
    public class RunProgressionService
    {
        private readonly FightStartedEventChannel _fightStarted;
        private readonly BossManager _bossManager;
        private readonly BossFightEventChannel _bossFightStarted;

        private RunState _currentRun;
        private int _fightIndex;
        private Unit _player;

        public RunProgressionService(FightStartedEventChannel fightStarted,
                                     BossManager bossManager = null,
                                     BossFightEventChannel bossFightStarted = null)
        {
            this._fightStarted = fightStarted;
            this._bossManager = bossManager;
            this._bossFightStarted = bossFightStarted;
        }

        /// <summary>
        ///     Initializes the service with the current run state and active player.
        ///     Call this when starting or continuing a run.
        /// </summary>
        public void SetRun(RunState run, Unit player)
        {
            this._currentRun = run;
            this._player = player;
            this._fightIndex = run.fightIndex;
        }

        /// <summary>
        ///     Raises the fight-started event with the current fight index, raises the boss-fight event
        ///     when the fight is a boss fight, increments the index, and saves state.
        ///     On boss fights the <see cref="BossFightEventChannel"/> is raised <em>before</em>
        ///     <see cref="FightStartedEventChannel"/> so that subscribers such as
        ///     <see cref="CombatOrchestrator"/> can prepare the boss unit before the fight begins.
        ///     The saved <c>fightIndex</c> records the next fight to play, so the run resumes correctly on reload.
        ///     Subscribe this to the <c>requestNextFight</c> event channel.
        /// </summary>
        public void HandleNextFight()
        {
            if (this._bossManager != null && this._bossManager.IsBossFight(this._fightIndex))
            {
                var boss = this._bossManager.GetBoss(this._fightIndex);
                if (boss != null)
                    this._bossFightStarted?.Raise(boss);
            }

            this._fightStarted?.Raise(this._player, this._fightIndex);
            this._fightIndex++;
            Save();
        }

        private void Save()
        {
            if (this._currentRun == null)
                return;

            this._currentRun.fightIndex = this._fightIndex;
            this._currentRun.player = this._player;
            SaveService.Save(this._currentRun);
        }
    }
}
