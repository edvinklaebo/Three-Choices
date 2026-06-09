using System;
using System.Collections.Generic;

using UnityEngine;

namespace Core.Progression
{
    /// <summary>
    /// Defines a single metaprogression unlock entry.
    /// Stored inside <see cref="ProgressionConfig"/> for data-driven balancing.
    /// </summary>
    [Serializable]
    public class ProgressionUnlockDefinition
    {
        [SerializeField] private string _id = string.Empty;
        [SerializeField] private string _name = "New Unlock";
        [TextArea] [SerializeField] private string _description = string.Empty;
        [Min(0)] [SerializeField] private int _unlockCost;
        [SerializeField] private ProgressionUnlockCategory _category = ProgressionUnlockCategory.Character;
        [SerializeField] private List<string> _dependencies = new();

        public string Id => _id;
        public string Name => _name;
        public string Description => _description;
        public int UnlockCost => _unlockCost;
        public ProgressionUnlockCategory Category => _category;
        public IReadOnlyList<string> Dependencies => _dependencies;

        public void Initialize(string id, string name, string description, int unlockCost,
                               ProgressionUnlockCategory category, List<string> dependencies = null)
        {
            _id = id;
            _name = name;
            _description = description;
            _unlockCost = Mathf.Max(0, unlockCost);
            _category = category;
            _dependencies = dependencies ?? new List<string>();
        }

    }
}
