using System.Collections;
using UnityEngine;

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
    [SerializeField] private EnemyFactory _enemyFactory;
    [SerializeField] private CombatSystemService _combatSystem;
    [SerializeField] private CombatView _combatView;
    
    private UIService _uiService;
    private SFXService _sfxService;
    private VFXService _vfxService;
    private AnimationService _animationService;
    private AnimationContext _animationContext;

    private bool _isFighting;
    
    private void Awake()
    {
        if (_animationRunner == null)
            Log.Error("CombatController: animationRunner is not assigned. Assign it in the Inspector.");
        if (_enemyFactory == null)
            Log.Error("CombatController: enemyFactory is not assigned. Assign it in the Inspector.");
        if (_combatSystem == null)
            Log.Error("CombatController: combatSystem is not assigned. Assign it in the Inspector.");
        if (_hideDraftUI == null)
            Log.Error("CombatController: hideDraftUI event channel is not assigned. Draft UI will not be hidden.");
        
        
        _uiService = new  UIService();
        _sfxService = new  SFXService();
        _vfxService = new  VFXService();
        _animationService = new AnimationService();
        _animationContext = new AnimationContext(_animationService, _uiService, _vfxService, _sfxService);
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
        
        // Hide draft UI before combat animations start
        _hideDraftUI?.Raise();

        var enemy = _enemyFactory.Create(fightIndex);
        CombatLogger.Instance.RegisterUnit(enemy);

        _combatView.Initialize(player, enemy);

        var bindings = _combatView.BuildBindings(player, enemy);
        
        _uiService.SetBindings(bindings);

        // Run combat logic (pure, deterministic)
        var actions = _combatSystem.RunFight(player, enemy);

        // Queue actions for animation
        _animationRunner.EnqueueRange(actions);
        _animationRunner.PlayAll(_animationContext);

        // Wait for animations to complete
        yield return _animationRunner.WaitForCompletion();

        // Hide combat view before showing draft UI
        if (_combatView)
            _combatView.Hide();

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