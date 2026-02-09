public class Poison : Passive
{
    private Unit target;
    private int damagePerTurn;
    private int turns;

    public Poison(Unit target, int damage, int turns)
    {
        this.target = target;
        damagePerTurn = damage;
        this.turns = turns;
    }

    public void OnTurnStart()
    {
        if (turns <= 0) return;

        target.ApplyDamage(damagePerTurn);
        turns--;
    }
}