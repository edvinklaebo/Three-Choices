using System;
using System.Collections.Generic;

[Serializable]
public class Unit
{
    public string Name;
    public Stats Stats;

    public List<IAbility> Abilities = new();
    public List<Passive> Passives = new();

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
}