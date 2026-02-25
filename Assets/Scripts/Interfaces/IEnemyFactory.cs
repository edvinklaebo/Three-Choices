/// <summary>
/// Contract for enemy creation, enabling seed-based generation, difficulty modifiers, and test injection.
/// </summary>
public interface IEnemyFactory
{
    Unit Create(int fightIndex);
}
