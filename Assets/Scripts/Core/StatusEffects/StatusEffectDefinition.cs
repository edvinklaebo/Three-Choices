using UnityEngine;

namespace Core.StatusEffects
{
    /// <summary>
    ///     ScriptableObject that holds display metadata for a status effect.
    ///     Contains name, description, icon, and color for use in the UI.
    /// </summary>
    [CreateAssetMenu(menuName = "Status Effects/Status Effect Definition")]
    public class StatusEffectDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;

        [TextArea]
        [SerializeField] private string _description;

        [Header("Visuals")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private Color _color = Color.white;

        // ---- Public read-only accessors ----
        public string Id => _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public Color Color => _color;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_id))
                Debug.LogWarning($"StatusEffectDefinition '{name}': Id must not be empty.", this);
        }

        public void EditorInit(string id, string displayName, string description, Color color, Sprite icon = null)
        {
            _id = id;
            _displayName = displayName;
            _description = description;
            _color = color;
            _icon = icon;
        }
#endif
    }
}
