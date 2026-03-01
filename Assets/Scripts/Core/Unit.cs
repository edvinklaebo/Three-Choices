using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Unit
{
    public string Name;
    public Stats Stats;
    public Sprite Portrait;

    public bool IsDead { get; private set; }

    [SerializeReference] public List<IAbility> Abilities = new();
    [SerializeReference] public List<IPassive> Passives = new();
    [SerializeReference] public List<IStatusEffect> StatusEffects = new();

    public Unit(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     Re-attaches all passives to this unit. Should be called after deserialization.
    ///     Passives are attached in priority order (lowest value first) so high-priority effects
    ///     subscribe to events before lower-priority ones.
    /// </summary>
    public void RestorePlayerState()
    {
        Log.Info($"[Unit] Restoring player state for {Name}");

        Passives.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        foreach (var passive in Passives)
        {
            if (passive == null)
            {
                Log.Warning($"[Unit] Null passive found in {Name}'s passive list");
                continue;
            }

            passive.OnAttach(this);
            Log.Info($"[Unit] Attached passive: {passive.GetType().Name}");
        }

        Log.Info(
            $"[Unit] Player state restored - Abilities: {Abilities.Count}, Passives: {Passives.Count}, StatusEffects: {StatusEffects.Count}");
    }

    public event Action<Unit, Unit, int> Damaged;
    public event Action<Unit, Unit, int> OnHit;
    public event Action<Unit, int, int> HealthChanged;
    public event Action<Unit, int> Healed;
    /// <summary>
    /// Raised before the unit dies. Subscribers can cancel the death by setting
    /// <see cref="DyingEventArgs.Cancelled"/> and providing a <see cref="DyingEventArgs.ReviveHp"/>.
    /// </summary>
    public event Action<Unit, DyingEventArgs> Dying;
    public event Action<Unit> Died;
    public event Action<Unit, IStatusEffect, bool> StatusEffectApplied;

    private const int MinReviveHp = 1;

    private void Die()
    {
        if (IsDead)
            return;

        var args = new DyingEventArgs();
        Dying?.Invoke(this, args);

        if (args.Cancelled)
        {
            Stats.CurrentHP = Math.Max(MinReviveHp, Math.Min(args.ReviveHp, Stats.MaxHP));
            HealthChanged?.Invoke(this, Stats.CurrentHP, Stats.MaxHP);
            Log.Info($"[Unit] {Name} death cancelled, restored to {Stats.CurrentHP} HP");
            return;
        }

        IsDead = true;

        Died?.Invoke(this);
    }

    /// <summary>
    /// Revives a dead unit with the specified HP.
    /// Does nothing if the unit is still alive.
    /// </summary>
    public void Revive(int hp)
    {
        if (!IsDead)
            return;

        IsDead = false;
        Stats.CurrentHP = Math.Max(MinReviveHp, Math.Min(hp, Stats.MaxHP));
        HealthChanged?.Invoke(this, Stats.CurrentHP, Stats.MaxHP);

        Log.Info($"[Unit] {Name} revived with {Stats.CurrentHP} HP");
    }

    public void RaiseOnHit(Unit target, int damage)
    {
        OnHit?.Invoke(this, target, damage);
    }

    public void ApplyDamage(Unit attacker, int damage)
    {
        if (IsDead || damage <= 0)
            return;

        var previousHp = Stats.CurrentHP;
        Stats.CurrentHP = Math.Max(0, Stats.CurrentHP - damage);

        Damaged?.Invoke(this, attacker, damage);
        attacker?.OnHit?.Invoke(attacker, this, damage);
        HealthChanged?.Invoke(this, Stats.CurrentHP, Stats.MaxHP);

        if (previousHp > 0 && Stats.CurrentHP == 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0)
            return;

        var actualHeal = Math.Min(Stats.MaxHP - Stats.CurrentHP, amount);
        Stats.CurrentHP += actualHeal;
        HealthChanged?.Invoke(this, Stats.CurrentHP, Stats.MaxHP);

        if (actualHeal > 0)
            Healed?.Invoke(this, actualHeal);
    }

    public void ApplyStatus(IStatusEffect effect)
    {
        if (IsDead || effect == null)
            return;
        
        IStatusEffect existing = null;

        for (var i = 0; i < StatusEffects.Count; i++)
        {
            if (StatusEffects[i].Id != effect.Id) 
                continue;
            existing = StatusEffects[i];
            break;
        }

        if (existing != null)
        {
            Log.Info("Status effect stacked", new
            {
                target = Name,
                effectId = effect.Id,
                oldStacks = existing.Stacks,
                addedStacks = effect.Stacks,
                newStacks = existing.Stacks + effect.Stacks
            });

            existing.AddStacks(effect);
            StatusEffectApplied?.Invoke(this, existing, true);
            return;
        }

        StatusEffects.Add(effect);
        effect.OnApply(this);
        StatusEffectApplied?.Invoke(this, effect, false);
    }

    public void TickStatusesTurnStart()
    {
        Tick(e => e.OnTurnStart(this));
    }

    public void TickStatusesTurnEnd()
    {
        Tick(e => e.OnTurnEnd(this));
    }

    private void Tick(Func<IStatusEffect, int> tickAction)
    {
        for (var i = StatusEffects.Count - 1; i >= 0; i--)
        {
            if (IsDead)
                break;
            
            var e = StatusEffects[i];
            var damage = tickAction(e);

            if (damage > 0)
                ApplyDamage(null, damage);

            if (e.Duration <= 0)
            {
                e.OnExpire(this);
                StatusEffects.RemoveAt(i);
            }
        }
    }
}