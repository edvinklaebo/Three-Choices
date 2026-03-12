using System;

using Core;

using Interfaces;

using UnityEngine;

using Utils;

namespace Examples
{
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
                this._owner = owner;
                this._damagePerStack = damagePerStack;
                this._maxStacks = maxStacks;
            }

            public int Priority => 150;

            public void Modify(DamageContext ctx)
            {
                if (ctx.Source != this._owner) return;

                // Reset combo if target changed
                if (ctx.Target != this._lastTarget)
                {
                    this._currentStacks = 0;
                    this._lastTarget = ctx.Target;
                }

                // Apply combo bonus
                if (this._currentStacks > 0)
                {
                    var bonus = 1f + this._currentStacks * this._damagePerStack;
                    ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * bonus);
                }

                // Increment stacks for next hit
                this._currentStacks = Mathf.Min(this._currentStacks + 1, this._maxStacks);
            }

            public void ResetCombo()
            {
                this._currentStacks = 0;
                this._lastTarget = null;
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
                this._owner = owner;
                this._bonusPerTurn = bonusPerTurn;
                this._maxTurns = maxTurns;
            }

            public int Priority => 180;

            public void Modify(DamageContext ctx)
            {
                if (ctx.Source != this._owner) return;

                var bonus = 1f + this._turnsActive * this._bonusPerTurn;
                ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * bonus);

                this._turnsActive = Mathf.Min(this._turnsActive + 1, this._maxTurns);
            }

            public void DecayMomentum()
            {
                this._turnsActive = Mathf.Max(0, this._turnsActive - 1);
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
                this._owner = owner;
                this._maxResource = maxResource;
                CurrentResource = maxResource;
                this._damagePerResourceSpent = damagePerResourceSpent;
            }

            public int CurrentResource { get; private set; }

            public int Priority => 220; // Late, final multiplier

            public void Modify(DamageContext ctx)
            {
                if (ctx.Source != this._owner) return;
                if (CurrentResource <= 0) return;

                // Spend all available resource for maximum damage
                var resourceSpent = CurrentResource;
                var bonus = 1f + resourceSpent * this._damagePerResourceSpent;
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
                CurrentResource = Mathf.Min(CurrentResource + amount, this._maxResource);
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
                this._owner = owner;
                this._thresholds = thresholds;

                // Sort by threshold descending for efficient lookup
                Array.Sort(this._thresholds, (a, b) => b.Item1.CompareTo(a.Item1));
            }

            public int Priority => 190;

            public void Modify(DamageContext ctx)
            {
                if (ctx.Source != this._owner) return;

                var hpPercent = (float)this._owner.Stats.CurrentHP / this._owner.Stats.MaxHP;

                // Find first matching threshold
                foreach (var threshold in this._thresholds)
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
                this._owner = owner;
                this._heavyMultiplier = heavyMultiplier;
                this._lightMultiplier = lightMultiplier;
                this._isHeavyAttack = true; // Start with heavy
            }

            public int Priority => 160;

            public void Modify(DamageContext ctx)
            {
                if (ctx.Source != this._owner) return;

                var multiplier = this._isHeavyAttack ? this._heavyMultiplier : this._lightMultiplier;
                ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * multiplier);

                // Alternate for next attack
                this._isHeavyAttack = !this._isHeavyAttack;

                Log.Info("Alternating attack", new
                {
                    type = this._isHeavyAttack ? "light" : "heavy", // Shows what NEXT will be
                    multiplier,
                    damage = ctx.FinalValue
                });
            }
        }
    }
}