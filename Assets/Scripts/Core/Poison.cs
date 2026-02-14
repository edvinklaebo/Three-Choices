public class Poison : Passive, IStatusEffect, IPassive
{
    public string Id => "Poison";
    public int Stacks { get; private set; }
    public int Duration { get; private set; }

    private readonly int passiveStacks;
    private readonly int passiveDuration;
    private bool isPassiveMode;

    // Constructor for status effect usage
    public Poison(int stacks, int duration)
    {
        Stacks = stacks;
        Duration = duration;
        isPassiveMode = false;
    }

    // Constructor for passive usage
    public Poison(Unit owner, int stacks = 2, int duration = 3)
    {
        Owner = owner;
        this.passiveStacks = stacks;
        this.passiveDuration = duration;
        isPassiveMode = true;

        owner.Damaged += OnDamaged;
    }

    // IStatusEffect implementation
    public void OnApply(Unit target)
    {
        Log.Info("Poison applied", new
        {
            target = target.Name,
            stacks = Stacks,
            duration = Duration
        });
    }

    public void OnTurnStart(Unit target)
    {
        Log.Info("Poison ticking", new
        {
            target = target.Name,
            damage = Stacks,
            duration = Duration,
            hpBefore = target.Stats.CurrentHP
        });

        target.ApplyDirectDamage(Stacks);
        Duration--;

        Log.Info("Poison damage applied", new
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
        Log.Info("Poison expired", new
        {
            target = target.Name
        });
    }

    public void AddStacks(int amount)
    {
        Stacks += amount;
    }

    // IPassive implementation
    public void OnTurnStart(Unit self, Unit enemy)
    {
        // Passive behavior - could be used for other turn-based effects
    }

    // Passive behavior - applies poison when owner takes damage
    private void OnDamaged(Unit attacker, int damageTaken)
    {
        if (attacker == null) 
            return;

        Log.Info("Poison passive triggered", new
        {
            defender = Owner.Name,
            attacker = attacker.Name,
            poisonStacks = passiveStacks,
            poisonDuration = passiveDuration
        });

        attacker.ApplyStatus(new Poison(passiveStacks, passiveDuration));
    }
}
