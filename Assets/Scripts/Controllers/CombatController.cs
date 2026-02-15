using System.Collections;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    [SerializeField] private VoidEventChannel requestNextFight;
    [SerializeField] private VoidEventChannel fightEnded;
    [SerializeField] private RunController runController;
    [SerializeField] private CombatAnimationRunner animationRunner;
    [SerializeField] private CombatView combatView;

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

        // Find combat view
        if (combatView == null)
        {
            combatView = FindFirstObjectByType<CombatView>();
            if (combatView == null)
            {
                Log.Warning("CombatController: CombatView not found in scene. Animations and UI may not work correctly.");
            }
        }

        // Initialize animation context with service placeholders
        var animService = new AnimationService();
        var uiService = new UIService();

        // Wire services to combat view
        if (combatView != null)
        {
            animService.SetCombatView(combatView);
            uiService.SetCombatView(combatView);
        }

        _animationContext = new AnimationContext(
            animService,
            uiService,
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
        // Hide draft UI before combat animations start
        if (DraftUI.Instance != null)
        {
            DraftUI.Instance.Hide(animated: false);
        }

        var enemy = EnemyFactory.Create(fightIndex);

        // Initialize combat view with combatants
        if (combatView != null)
        {
            combatView.Initialize(player, enemy);
        }

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