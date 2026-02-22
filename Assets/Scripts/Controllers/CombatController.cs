using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class CombatController : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private VoidEventChannel requestNextFight;
    [SerializeField] private VoidEventChannel fightEnded;
    [SerializeField] private VoidEventChannel combatEndedWithPlayerDeath;
    [SerializeField] private VoidEventChannel _hideDraftUI;
    [SerializeField] private FightStartedEventChannel _fightStarted;
    
    [Header("References")]
    [SerializeField] private CombatAnimationRunner _animationRunner;
    [SerializeField] private CombatServicesInstaller _servicesInstaller;
    [SerializeField] private EnemyFactory _enemyFactory;
    [SerializeField] private CombatSystemService _combatSystem;

    private bool _isFighting;
    
    private void Awake()
    {
        if (_animationRunner == null)
            Log.Error("CombatController: animationRunner is not assigned. Assign it in the Inspector.");
        if (_servicesInstaller == null)
            Log.Error("CombatController: servicesInstaller is not assigned. Assign it in the Inspector.");
        if (_enemyFactory == null)
            Log.Error("CombatController: enemyFactory is not assigned. Assign it in the Inspector.");
        if (_combatSystem == null)
            Log.Error("CombatController: combatSystem is not assigned. Assign it in the Inspector.");
        if (_hideDraftUI == null)
            Log.Warning("CombatController: hideDraftUI event channel is not assigned. Draft UI will not be hidden.");
    }

    private void Start()
    {
        requestNextFight.Raise();
    }

    private void OnEnable()
    {
        if (_fightStarted != null) 
            _fightStarted.OnRaised += HandleStartFight;
    }

    private void OnDisable()
    {
        if (_fightStarted != null) 
            _fightStarted.OnRaised -= HandleStartFight;
    }

    private void HandleStartFight(Unit player, int fightIndex)
    {
        if(_isFighting)
            return;
        
        StartCoroutine(StartFightCoroutine(player, fightIndex));
    }

    private IEnumerator StartFightCoroutine(Unit player, int fightIndex)
    {
        _isFighting = true;
        // Prevent re-entrancy: if a previous fight is still animating (e.g. from a duplicate
        // _fightStarted event), wait until it finishes before initialising the next fight.
        // Safe from deadlock because Run() has no dependency on this coroutine completing.
        if (_animationRunner.IsRunning)
            yield return _animationRunner.WaitForCompletion();

        // Hide draft UI before combat animations start
        _hideDraftUI?.Raise();

        var enemy = _enemyFactory.Create(fightIndex);
        CombatLogger.Instance.RegisterUnit(enemy);

        var combatView = _servicesInstaller.CombatView;

        // Initialize combat view with combatants
        if (combatView) 
            combatView.Initialize(player, enemy);

        // Build deterministic unit→UI mapping once, now that CombatView is initialized
        if (combatView)
            _servicesInstaller.Context?.UI.SetBindings(combatView.BuildBindings(player, enemy));

        // Run combat logic (pure, deterministic)
        var actions = _combatSystem.RunFight(player, enemy);

        // Queue actions for animation
        _animationRunner.EnqueueRange(actions);
        _animationRunner.PlayAll(_servicesInstaller.Context);

        // Wait for animations to complete
        yield return _animationRunner.WaitForCompletion();

        // Hide combat view before showing draft UI
        if (combatView) 
            combatView.Hide();

        // Continue game flow
        if (player.Stats.CurrentHP <= 0)
        {
            // Raise event after death animation completes
            if (combatEndedWithPlayerDeath)
                combatEndedWithPlayerDeath.Raise();
            else
                Log.Warning("CombatController - combatEndedWithPlayerDeath event channel not assigned");
            yield break;
        }

        fightEnded.Raise();
        _isFighting = false;
    }
}