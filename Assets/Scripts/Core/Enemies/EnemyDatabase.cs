using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     ScriptableObject that holds all available enemy definitions.
///     Referenced by <see cref="EnemyFactory"/> to select enemies for fights.
///     Create via the asset menu: Game/Enemies/Enemy Database.
/// </summary>
[CreateAssetMenu(menuName = "Game/Enemies/Enemy Database")]
public class EnemyDatabase : ScriptableObject
{
    [SerializeField] private List<EnemyDefinition> _enemies = new();

    public IReadOnlyList<EnemyDefinition> Enemies => _enemies;

#if UNITY_EDITOR
    public void EditorAddEnemy(EnemyDefinition definition)
    {
        _enemies.Add(definition);
    }
#endif
}
