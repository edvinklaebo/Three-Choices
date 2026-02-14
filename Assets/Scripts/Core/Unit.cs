using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class Unit
{
    public string Name;
    public Stats Stats;

    public List<IAbility> Abilities = new();
    public List<Passive> Passives = new();
    public List<IStatusEffect> StatusEffects = new();

    public event Action<Unit,int> Damaged;
    public event Action<Unit, int, int> HealthChanged;
    public event Action<Unit> Died;

    public bool isDead;
    
    public Unit(string name)
    {
        Name = name;
    }

    private void Die()
    {
        if (isDead) 
            return;
        
        isDead = true;

        Died?.Invoke(this);
    }
    
    public void ApplyDamage(Unit attacker, int damage)
    {
        if (isDead) 
            return;

        Stats.CurrentHP -= damage;

        Damaged?.Invoke(attacker, damage);
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

            existing.AddStacks(effect.Stacks);
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
        for (int i = StatusEffects.Count - 1; i >= 0; i--)
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