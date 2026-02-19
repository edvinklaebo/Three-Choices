using System.Collections;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    [SerializeField] private VoidEventChannel requestNextFight;
    [SerializeField] private VoidEventChannel fightEnded;
    [SerializeField] private VoidEventChannel combatEndedWithPlayerDeath;
    [SerializeField] private FightStartedEventChannel _fightStarted;
    [SerializeField] private CombatAnimationRunner animationRunner;
    [SerializeField] private CombatView combatView;

    private AnimationContext _animationContext;

    private void Awake()
    {
        if (animationRunner == null)
            Log.Error("CombatController: animationRunner is not assigned. Assign it in the Inspector.");
        if (combatView == null)
            Log.Error("CombatController: combatView is not assigned. Assign it in the Inspector.");
    }

    private void Start()
    {
        if (animationRunner == null || combatView == null) return;

        var animService = new AnimationService();
        var uiService = new UIService();

        animService.SetCombatView(combatView);
        uiService.SetCombatView(combatView);

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
        if (_fightStarted != null) _fightStarted.OnRaised += HandleStartFight;
    }

    private void OnDisable()
    {
        if (_fightStarted != null) _fightStarted.OnRaised -= HandleStartFight;
    }

    private void HandleStartFight(Unit player, int fightIndex)
    {
        StartCoroutine(StartFightCoroutine(player, fightIndex));
    }

    private IEnumerator StartFightCoroutine(Unit player, int fightIndex)
    {
        // Hide draft UI before combat animations start
        if (DraftUI.Instance != null) DraftUI.Instance.Hide(false);

        var enemy = EnemyFactory.Create(fightIndex);

        // Initialize combat view with combatants
        if (combatView != null) combatView.Initialize(player, enemy);

        // Run combat logic (pure, deterministic)
        var actions = CombatSystem.RunFight(player, enemy);

        // Queue actions for animation
        animationRunner.EnqueueRange(actions);
        animationRunner.PlayAll(_animationContext);

        // Wait for animations to complete
        yield return new WaitUntil(() => !animationRunner.IsRunning);

        // Hide combat view before showing draft UI
        if (combatView != null) combatView.Hide();

        // Continue game flow
        if (player.Stats.CurrentHP <= 0)
        {
            // Raise event after death animation completes
            if (combatEndedWithPlayerDeath != null)
                combatEndedWithPlayerDeath.Raise();
            else
                Log.Warning("CombatController - combatEndedWithPlayerDeath event channel not assigned");
            yield break;
        }

        fightEnded.Raise();
    }
}