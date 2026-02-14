public class PoisonEffect : IStatusEffect
{
    public string Id => "Poison";
    public int Stacks { get; private set; }
    public int Duration { get; private set; }

    public PoisonEffect(int stacks, int duration)
    {
        Stacks = stacks;
        Duration = duration;
    }

    public void OnApply(Unit target)
    {
        // No special behavior on apply
    }

    public void OnTurnStart(Unit target)
    {
        target.ApplyDirectDamage(Stacks);
        Duration--;
    }

    public void OnTurnEnd(Unit target)
    {
        // No behavior on turn end
    }

    public void OnExpire(Unit target)
    {
        // No special behavior on expiration
    }

    public void AddStacks(int amount)
    {
        Stacks += amount;
    }
}
