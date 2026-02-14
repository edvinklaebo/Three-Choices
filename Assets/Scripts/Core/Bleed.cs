public class Bleed : Passive, IStatusEffect
{
    private readonly int passiveDuration;

    private readonly int passiveStacks;

    // Constructor for status effect usage
    public Bleed(int stacks, int duration)
    {
        Stacks = stacks;
        Duration = duration;
    }

    // Constructor for passive usage
    public Bleed(Unit owner, int stacks = 2, int duration = 3)
    {
        Owner = owner;
        passiveStacks = stacks;
        passiveDuration = duration;

        owner.Damaged += OnDamaged;
    }

    public string Id => "Bleed";
    public int Stacks { get; private set; }
    public int Duration { get; private set; }

    // IStatusEffect implementation
    public void OnApply(Unit target)
    {
        Log.Info("Bleed applied", new
        {
            target = target.Name,
            stacks = Stacks,
            duration = Duration
        });
    }

    public void OnTurnStart(Unit target)
    {
        Log.Info("Bleed ticking", new
        {
            target = target.Name,
            damage = Stacks,
            duration = Duration,
            hpBefore = target.Stats.CurrentHP
        });

        target.ApplyDirectDamage(Stacks);
        Duration--;

        Log.Info("Bleed damage applied", new
        {
            target = target.Name,
            hpAfter = target.Stats.CurrentHP,
            remainingDuration = Duration
        });
    }

    public void OnTurnEnd(Unit target)
    {
        // No behavior on turn end
    }

    public void OnExpire(Unit target)
    {
        Log.Info("Bleed expired", new
        {
            target = target.Name
        });
    }

    public void AddStacks(int amount)
    {
        Stacks += amount;
    }

    // Passive behavior - applies bleed when owner takes damage
    private void OnDamaged(Unit attacker, int damageTaken)
    {
        if (attacker == null)
            return;

        Log.Info("Bleed passive triggered", new
        {
            defender = Owner.Name,
            attacker = attacker.Name,
            bleedStacks = passiveStacks,
            bleedDuration = passiveDuration
        });

        attacker.ApplyStatus(new Bleed(passiveStacks, passiveDuration));
    }
}
