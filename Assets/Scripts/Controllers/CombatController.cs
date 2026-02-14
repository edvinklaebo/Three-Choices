using System.Collections;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    [SerializeField] private VoidEventChannel requestNextFight;
    [SerializeField] private VoidEventChannel fightEnded;
    [SerializeField] private RunController runController;
    [SerializeField] private CombatAnimationRunner animationRunner;

    private AnimationContext _animationContext;

    private void Start()
    {
        runController = FindFirstObjectByType<RunController>();
        if (runController == null)
        {
            Log.Error("No RunController found! Did you forget to start new run?");
            return;
        }

        // Find or create animation runner
        if (animationRunner == null)
        {
            animationRunner = FindFirstObjectByType<CombatAnimationRunner>();
            if (animationRunner == null)
            {
                var runnerObj = new GameObject("CombatAnimationRunner");
                animationRunner = runnerObj.AddComponent<CombatAnimationRunner>();
            }
        }

        // Initialize animation context with service placeholders
        _animationContext = new AnimationContext(
            new AnimationService(),
            new UIService(),
            new VFXService(),
            new SFXService()
        );

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
        StartCoroutine(StartFightCoroutine(runController.Player, runController.CurrentRun.fightIndex));
    }

    private IEnumerator StartFightCoroutine(Unit player, int fightIndex)
    {
        var enemy = EnemyFactory.Create(fightIndex);

        // Run combat logic (pure, deterministic)
        var actions = CombatSystem.RunFight(player, enemy);

        // Queue actions for animation
        animationRunner.EnqueueRange(actions);
        animationRunner.PlayAll(_animationContext);

        // Wait for animations to complete
        yield return new WaitUntil(() => !animationRunner.IsRunning);

        // Continue game flow
        if (player.Stats.CurrentHP <= 0)
            yield break;

        fightEnded.Raise();
    }
}