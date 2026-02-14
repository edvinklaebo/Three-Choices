using UnityEngine;

public class CombatController : MonoBehaviour
{
    [SerializeField] private VoidEventChannel requestNextFight;
    [SerializeField] private VoidEventChannel fightEnded;
    [SerializeField] private RunController runController;

    private void Start()
    {
        runController = FindFirstObjectByType<RunController>();
        if (runController == null)
        {
            Log.Error("No RunController found! Did you forget to start new run?");
            return;
        }

        requestNextFight.Raise();
    }

    private void OnEnable()
    {
        requestNextFight.OnRaised += HandleStartFight;
    }

    private void OnDisable()
    {
        requestNextFight.OnRaised -= HandleStartFight;
    }

    private void HandleStartFight()
    {
        StartFight(runController.Player, runController.CurrentRun.fightIndex);
    }

    private void StartFight(Unit player, int fightIndex)
    {
        var enemy = EnemyFactory.Create(fightIndex);

        CombatSystem.RunFight(player, enemy);

        if (player.Stats.CurrentHP <= 0)
            return;

        fightEnded.Raise();
    }
}