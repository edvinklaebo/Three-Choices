using System;

using UnityEngine;

namespace Core
{
    public enum ObtainedItemCategory
    {
        Upgrades,
        Abilities,
        Passives,
        Artifacts
    }

    /// <summary>
    /// A run-owned snapshot of an acquired definition, persisted without asset references.
    /// Count records acquisitions, not the current strength of the runtime effect.
    /// </summary>
    [Serializable]
    public class ObtainedItem
    {
        [SerializeField] private string _id = string.Empty;
        [SerializeField] private string _displayName = string.Empty;
        [SerializeField] private string _description = string.Empty;
        [SerializeField] private ObtainedItemCategory _category = ObtainedItemCategory.Upgrades;
        [SerializeField] private int _count = 1;

        public string Id => _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public ObtainedItemCategory Category => _category;
        public int Count => _count;

        internal bool IsValid => !string.IsNullOrWhiteSpace(_id) &&
                                 !string.IsNullOrWhiteSpace(_displayName) && _count > 0 &&
                                 _category >= ObtainedItemCategory.Upgrades &&
                                 _category <= ObtainedItemCategory.Artifacts;

        public ObtainedItem(string id, string displayName, string description, ObtainedItemCategory category)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Obtained item must have an identity.", nameof(id));
            if (category < ObtainedItemCategory.Upgrades || category > ObtainedItemCategory.Artifacts)
                throw new ArgumentOutOfRangeException(nameof(category));

            _id = id;
            _displayName = string.IsNullOrWhiteSpace(displayName) ? id : displayName;
            _description = description ?? string.Empty;
            _category = category;
        }

        internal void IncrementCount()
        {
            _count++;
        }
    }
}
