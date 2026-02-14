public class Poison : Passive
{
    private readonly int stacks;
    private readonly int duration;

    public Poison(Unit owner, int stacks = 2, int duration = 3)
    {
        Owner = owner;
        this.stacks = stacks;
        this.duration = duration;

        owner.Damaged += OnDamaged;
    }

    private void OnDamaged(Unit attacker, int damageTaken)
    {
        if (attacker == null) 
            return;

        Log.Info("Poison passive triggered", new
        {
            defender = Owner.Name,
            attacker = attacker.Name,
            poisonStacks = stacks,
            poisonDuration = duration
        });

        attacker.ApplyStatus(new PoisonEffect(stacks, duration));
    }
}