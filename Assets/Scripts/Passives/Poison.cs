public class Poison : Passive
{
    private Unit target;
    private Unit source;
    private int damagePerTurn;
    private int turns;

    // TODO fix not self poisoning
    public Poison(Unit target, Unit source, int damage, int turns)
    {
        this.target = target;
        this.source = source;
        damagePerTurn = damage;
        this.turns = turns;
    }

    public void OnTurnStart()
    {
        if (turns <= 0) return;

        //target.ApplyDamage(this.source, damagePerTurn);
        turns--;
    }
}