using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     ScriptableObject that holds all available boss definitions.
///     Referenced by <see cref="BossManager"/> to select bosses for fights.
///     Create via the asset menu: Game/Boss/Boss Registry.
/// </summary>
[CreateAssetMenu(menuName = "Game/Boss/Boss Registry")]
public class BossRegistry : ScriptableObject
{
    [SerializeField] private List<BossDefinition> _bosses = new List<BossDefinition>();

    public IReadOnlyList<BossDefinition> Bosses => _bosses;

#if UNITY_EDITOR
    public void EditorAddBoss(BossDefinition boss)
    {
        _bosses.Add(boss);
    }
#endif
}
