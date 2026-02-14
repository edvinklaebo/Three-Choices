public class DamageContext
{
    public readonly Unit Source;

    public int BaseValue;
    public int FinalValue;

    public bool IsCritical;
    public Unit Target;

    public DamageContext(Unit source, Unit target, int value)
    {
        Source = source;
        Target = target;
        BaseValue = value;
        FinalValue = value;
    }
}