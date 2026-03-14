using System.Collections.Generic;

using UnityEngine;

using Utils;

namespace Core.StatusEffects
{
    /// <summary>
    ///     ScriptableObject registry that maps status effect IDs to their definitions.
    ///     Assign all StatusEffectDefinition assets here so the UI can look them up by ID.
    ///     Lookups are O(1) via a lazily-built dictionary.
    /// </summary>
    [CreateAssetMenu(menuName = "Status Effects/Status Effect Library")]
    public class StatusEffectLibrary : ScriptableObject
    {
        [SerializeField] private List<StatusEffectDefinition> _definitions = new();

        private Dictionary<string, StatusEffectDefinition> _lookup;

        private void OnEnable() => _lookup = null;

        /// <summary>
        ///     Returns the definition for the given effect ID, or null if not found.
        /// </summary>
        public StatusEffectDefinition GetDefinition(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            BuildLookupIfNeeded();

            if (_lookup.TryGetValue(id, out var definition))
                return definition;

            Log.Warning($"StatusEffectLibrary: No definition found for effect id '{id}'");
            return null;
        }

        private void BuildLookupIfNeeded()
        {
            if (_lookup != null)
                return;

            _lookup = new Dictionary<string, StatusEffectDefinition>(_definitions.Count);
            for (var i = 0; i < _definitions.Count; i++)
            {
                var def = _definitions[i];
                if (def == null || string.IsNullOrEmpty(def.Id))
                    continue;

                if (!_lookup.TryAdd(def.Id, def))
                    Log.Warning($"StatusEffectLibrary: Duplicate definition id '{def.Id}' — ignoring.");
            }
        }

#if UNITY_EDITOR
        public void EditorAddDefinition(StatusEffectDefinition definition)
        {
            if (definition != null && !_definitions.Contains(definition))
            {
                _definitions.Add(definition);
                _lookup = null; // invalidate cache
            }
        }
#endif
    }
}
