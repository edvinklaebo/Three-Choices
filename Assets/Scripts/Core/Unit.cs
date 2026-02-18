using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Unit
{
    public string Name;
    public Stats Stats;

    public bool isDead;

    [SerializeReference] public List<IAbility> Abilities = new();
    [SerializeReference] public List<IPassive> Passives = new();
    [SerializeReference] public List<IStatusEffect> StatusEffects = new();

    public Unit(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     Re-attaches all passives to this unit. Should be called after deserialization.
    /// </summary>
    public void RestorePlayerState()
    {
        Log.Info($"[Unit] Restoring player state for {Name}");

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

    public event Action<Unit, int> Damaged;
    public event Action<Unit, int> OnHit;
    public event Action<Unit, int, int> HealthChanged;
    public event Action<Unit> Died;

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Died?.Invoke(this);
    }

    public void RaiseOnHit(Unit target, int damage)
    {
        OnHit?.Invoke(target, damage);
    }

    public void ApplyDamage(Unit attacker, int damage)
    {
        if (isDead)
            return;

        Stats.CurrentHP -= damage;

        Damaged?.Invoke(attacker, damage);
        attacker?.OnHit?.Invoke(this, damage);
        HealthChanged?.Invoke(this, Stats.CurrentHP, Stats.MaxHP);

        if (Stats.CurrentHP <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        Stats.CurrentHP = Math.Min(Stats.MaxHP, Stats.CurrentHP + amount);
        HealthChanged?.Invoke(this, Stats.CurrentHP, Stats.MaxHP);
    }

    public void ApplyDirectDamage(int damage)
    {
        if (isDead)
            return;

        Stats.CurrentHP -= damage;

        HealthChanged?.Invoke(this, Stats.CurrentHP, Stats.MaxHP);

        if (Stats.CurrentHP <= 0)
            Die();
    }

    public void ApplyStatus(IStatusEffect effect)
    {
        var existing = StatusEffects.FirstOrDefault(e => e.Id == effect.Id);

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
            return;
        }

        StatusEffects.Add(effect);
        effect.OnApply(this);
    }

    public void TickStatusesTurnStart()
    {
        Tick(e => e.OnTurnStart(this));
    }

    public void TickStatusesTurnEnd()
    {
        Tick(e => e.OnTurnEnd(this));
    }

    private void Tick(Action<IStatusEffect> action)
    {
        for (var i = StatusEffects.Count - 1; i >= 0; i--)
        {
            var e = StatusEffects[i];
            action(e);

            if (e.Duration <= 0)
            {
                e.OnExpire(this);
                StatusEffects.RemoveAt(i);
            }
        }
    }
}