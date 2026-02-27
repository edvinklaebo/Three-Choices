using System;
using UnityEngine;

/// <summary>
///     Example modifier implementations demonstrating various decision-density patterns.
///     These examples show 5 variations of combat modifiers that increase tactical depth.
/// </summary>
public class ModifierExamples
{
    // ==================== EXAMPLE USAGE ====================

    public static void DemonstrateVariations()
    {
        var player = new Unit("Player")
        {
            Stats = new Stats { MaxHP = 100, CurrentHP = 100, AttackPower = 10, Armor = 0, Speed = 5 }
        };

        var enemy = new Unit("Enemy")
        {
            Stats = new Stats { MaxHP = 100, CurrentHP = 100, AttackPower = 5, Armor = 0, Speed = 5 }
        };

        // Example: Combo build focused on sustained damage to single target
        var combo = new ComboModifier(player, 0.15f, 5); // +15% per stack, max 5
        DamagePipeline.Register(combo);

        // Example: Momentum build for aggressive play
        var momentum = new MomentumModifier(player, 0.1f, 10); // +10% per turn, max 10
        DamagePipeline.Register(momentum);

        // Example: Resource management build
        var resource = new ResourceAmplificationModifier(player, 10, 0.2f); // 10 max, +20% per point
        DamagePipeline.Register(resource);

        // Example: High-risk threshold build
        var threshold = new ThresholdModifier(player,
            (0.25f, 3.0f), // 3x damage below 25% HP
            (0.50f, 2.0f), // 2x damage below 50% HP
            (0.75f, 1.5f) // 1.5x damage below 75% HP
        );
        DamagePipeline.Register(threshold);

        // Example: Rhythmic alternating build
        var alternating = new AlternatingPatternModifier(player, 2.0f, 0.5f); // Heavy/light pattern
        DamagePipeline.Register(alternating);
    }
    // ==================== VARIATION 1: COMBO SYSTEM ====================

    /// <summary>
    ///     Increases damage based on consecutive hits against the same target.
    ///     Resets on target change or miss turn.
    /// </summary>
    public class ComboModifier : IDamageModifier
    {
        private readonly float _damagePerStack;
        private readonly int _maxStacks;

        private readonly Unit _owner;
        private int _currentStacks;
        private Unit _lastTarget;

        public ComboModifier(Unit owner, float damagePerStack, int maxStacks)
        {
            _owner = owner;
            _damagePerStack = damagePerStack;
            _maxStacks = maxStacks;
        }

        public int Priority => 150;

        public void Modify(DamageContext ctx)
        {
            if (ctx.Source != _owner) return;

            // Reset combo if target changed
            if (ctx.Target != _lastTarget)
            {
                _currentStacks = 0;
                _lastTarget = ctx.Target;
            }

            // Apply combo bonus
            if (_currentStacks > 0)
            {
                var bonus = 1f + _currentStacks * _damagePerStack;
                ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * bonus);
            }

            // Increment stacks for next hit
            _currentStacks = Mathf.Min(_currentStacks + 1, _maxStacks);
        }

        public void ResetCombo()
        {
            _currentStacks = 0;
            _lastTarget = null;
        }
    }

    // ==================== VARIATION 2: MOMENTUM SYSTEM ====================

    /// <summary>
    ///     Builds momentum over successive turns, increasing damage each turn.
    ///     Momentum decays when not attacking.
    /// </summary>
    public class MomentumModifier : IDamageModifier
    {
        private readonly float _bonusPerTurn;
        private readonly int _maxTurns;

        private readonly Unit _owner;
        private int _turnsActive;

        public MomentumModifier(Unit owner, float bonusPerTurn, int maxTurns)
        {
            _owner = owner;
            _bonusPerTurn = bonusPerTurn;
            _maxTurns = maxTurns;
        }

        public int Priority => 180;

        public void Modify(DamageContext ctx)
        {
            if (ctx.Source != _owner) return;

            var bonus = 1f + _turnsActive * _bonusPerTurn;
            ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * bonus);

            _turnsActive = Mathf.Min(_turnsActive + 1, _maxTurns);
        }

        public void DecayMomentum()
        {
            _turnsActive = Mathf.Max(0, _turnsActive - 1);
        }
    }

    // ==================== VARIATION 3: RESOURCE SYSTEM ====================

    /// <summary>
    ///     Consumes a resource (e.g., mana, energy) to amplify damage.
    ///     Creates decision: use resource for big damage now or save for later?
    /// </summary>
    public class ResourceAmplificationModifier : IDamageModifier
    {
        private readonly float _damagePerResourceSpent;
        private readonly int _maxResource;

        private readonly Unit _owner;

        public ResourceAmplificationModifier(Unit owner, int maxResource, float damagePerResourceSpent)
        {
            _owner = owner;
            _maxResource = maxResource;
            CurrentResource = maxResource;
            _damagePerResourceSpent = damagePerResourceSpent;
        }

        public int CurrentResource { get; private set; }

        public int Priority => 220; // Late, final multiplier

        public void Modify(DamageContext ctx)
        {
            if (ctx.Source != _owner) return;
            if (CurrentResource <= 0) return;

            // Spend all available resource for maximum damage
            var resourceSpent = CurrentResource;
            var bonus = 1f + resourceSpent * _damagePerResourceSpent;
            ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * bonus);

            CurrentResource = 0;

            Log.Info("Resource amplification", new
            {
                spent = resourceSpent,
                bonus,
                damage = ctx.FinalValue
            });
        }

        public void GainResource(int amount)
        {
            CurrentResource = Mathf.Min(CurrentResource + amount, _maxResource);
        }
    }

    // ==================== VARIATION 4: HEALTH-THRESHOLD SYSTEM ====================

    /// <summary>
    ///     Multiple damage tiers based on health thresholds.
    ///     Creates risk/reward: more damage at lower HP.
    /// </summary>
    public class ThresholdModifier : IDamageModifier
    {
        private readonly Unit _owner;
        private readonly (float hpThreshold, float damageMultiplier)[] _thresholds;

        public ThresholdModifier(Unit owner, params (float threshold, float multiplier)[] thresholds)
        {
            _owner = owner;
            _thresholds = thresholds;

            // Sort by threshold descending for efficient lookup
            Array.Sort(_thresholds, (a, b) => b.Item1.CompareTo(a.Item1));
        }

        public int Priority => 190;

        public void Modify(DamageContext ctx)
        {
            if (ctx.Source != _owner) return;

            var hpPercent = (float)_owner.Stats.CurrentHP / _owner.Stats.MaxHP;

            // Find first matching threshold
            foreach (var threshold in _thresholds)
                if (hpPercent <= threshold.Item1)
                {
                    ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * threshold.Item2);
                    return;
                }
        }
    }

    // ==================== VARIATION 5: ALTERNATING PATTERN SYSTEM ====================

    /// <summary>
    ///     Alternates between two damage modes (e.g., heavy/light attacks).
    ///     Encourages strategic timing and pattern recognition.
    /// </summary>
    public class AlternatingPatternModifier : IDamageModifier
    {
        private readonly float _heavyMultiplier;
        private readonly float _lightMultiplier;

        private readonly Unit _owner;
        private bool _isHeavyAttack;

        public AlternatingPatternModifier(Unit owner, float heavyMultiplier, float lightMultiplier)
        {
            _owner = owner;
            _heavyMultiplier = heavyMultiplier;
            _lightMultiplier = lightMultiplier;
            _isHeavyAttack = true; // Start with heavy
        }

        public int Priority => 160;

        public void Modify(DamageContext ctx)
        {
            if (ctx.Source != _owner) return;

            var multiplier = _isHeavyAttack ? _heavyMultiplier : _lightMultiplier;
            ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * multiplier);

            // Alternate for next attack
            _isHeavyAttack = !_isHeavyAttack;

            Log.Info("Alternating attack", new
            {
                type = _isHeavyAttack ? "light" : "heavy", // Shows what NEXT will be
                multiplier,
                damage = ctx.FinalValue
            });
        }
    }
}