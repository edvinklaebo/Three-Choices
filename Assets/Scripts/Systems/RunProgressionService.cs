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
        _fightStarted = fightStarted;
        _bossManager = bossManager;
        _bossFightStarted = bossFightStarted;
    }

    /// <summary>
    ///     Initializes the service with the current run state and active player.
    ///     Call this when starting or continuing a run.
    /// </summary>
    public void SetRun(RunState run, Unit player)
    {
        _currentRun = run;
        _player = player;
        _fightIndex = run.fightIndex;
    }

    /// <summary>
    ///     Raises the fight-started event with the current fight index, raises the boss-fight event
    ///     when the fight is a boss fight, increments the index, and saves state.
    ///     The saved <c>fightIndex</c> records the next fight to play, so the run resumes correctly on reload.
    ///     Subscribe this to the <c>requestNextFight</c> event channel.
    /// </summary>
    public void HandleNextFight()
    {
        _fightStarted?.Raise(_player, _fightIndex);

        if (_bossManager != null && _bossManager.IsBossFight(_fightIndex))
        {
            var boss = _bossManager.GetBoss(_fightIndex);
            if (boss != null)
                _bossFightStarted?.Raise(boss);
        }

        _fightIndex++;
        Save();
    }

    private void Save()
    {
        if (_currentRun == null)
            return;

        _currentRun.fightIndex = _fightIndex;
        _currentRun.player = _player;
        SaveService.Save(_currentRun);
    }
}
