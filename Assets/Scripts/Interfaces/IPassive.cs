public interface IPassive
{
    /// <summary>
    /// Priority for passive ordering. Lower values = earlier execution and event subscription.
    /// 0: Highest priority (e.g. death prevention)
    /// 100: Normal passives
    /// 200+: Late passives
    /// </summary>
    int Priority { get; }

    void OnAttach(Unit owner);
    void OnDetach(Unit owner);
}