using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Core.Boss
{
    /// <summary>
    ///     ScriptableObject that holds all available boss definitions.
    ///     Referenced by <see cref="BossManager"/> to select bosses for fights.
    ///     Create via the asset menu: Game/Boss/Boss Registry.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Boss/Boss Registry")]
    public class BossPool : ScriptableObject
    {
        [SerializeField] private List<BossDefinition> _bosses = new();

        public IReadOnlyList<BossDefinition> Bosses => _bosses;

#if UNITY_EDITOR
        public void EditorAddBoss(BossDefinition boss)
        {
            _bosses.Add(boss);
        }

        [ContextMenu("Auto Populate")]
        private void AutoPopulate()
        {
            _bosses.Clear();

            var guids = AssetDatabase.FindAssets("t:BossDefinition");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var boss = AssetDatabase.LoadAssetAtPath<BossDefinition>(path);

                if (boss != null)
                    _bosses.Add(boss);
            }

            EditorUtility.SetDirty(this);
        }
#endif
    }
}
