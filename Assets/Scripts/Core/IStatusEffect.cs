public interface IStatusEffect
{
    string Id { get; }
    int Stacks { get; }
    int Duration { get; }
    
    void OnApply(Unit target);
    void OnTurnStart(Unit target);
    void OnTurnEnd(Unit target);
    void OnExpire(Unit target);
    void AddStacks(int amount);
}
