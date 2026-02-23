/// <summary>
/// Handles combat view lifecycle: setup, UI binding, and visibility.
/// Decouples view concerns from presentation sequencing in CombatPresentationCoordinator.
/// </summary>
public interface ICombatViewPresenter
{
    AnimationContext Context { get; }
    void Show(CombatResult result);
    void Hide();
}
