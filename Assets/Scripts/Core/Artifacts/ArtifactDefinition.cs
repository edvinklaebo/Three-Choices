using Interfaces;

using UnityEngine;

namespace Core.Artifacts
{
    [CreateAssetMenu(menuName = "Artifacts/Artifact")]
    public class ArtifactDefinition : ScriptableObject, IDraftable
    {
        [Header("Identity")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [TextArea] [SerializeField] private string _description;
        [SerializeField] private Sprite _icon;

        [Header("Rarity & Tags")]
        [SerializeField] private Rarity _rarity = Rarity.Common;
        [SerializeField] private ArtifactTag _tags = ArtifactTag.None;

        [Header("Effect")]
        [SerializeField] private ArtifactEffectType _effectType;
        [SerializeField] private ArtifactId _artifactId;

        [Header("Meta-Progression")]
        [SerializeField] private bool _lockedByDefault = true;

        // ---- Public read-only accessors ----
        public string Id => this._id;
        public string DisplayName => this._displayName;
        public string Description => this._description;
        public Sprite Icon => this._icon;
        public Rarity Rarity => this._rarity;
        public Rarity GetRarity() => this._rarity;
        public ArtifactTag Tags => this._tags;
        public ArtifactEffectType EffectType => this._effectType;
        public ArtifactId ArtifactId => this._artifactId;
        public bool LockedByDefault => this._lockedByDefault;

#if UNITY_EDITOR
        public void EditorInit(string id, string displayName, string description,
                               Rarity rarity, ArtifactTag tags, ArtifactEffectType effectType, bool lockedByDefault = true)
        {
            this._id = id;
            this._displayName = displayName;
            this._description = description;
            this._rarity = rarity;
            this._tags = tags;
            this._effectType = effectType;
            this._lockedByDefault = lockedByDefault;
        }

        public void EditorInit(ArtifactId artifactId, string displayName, string description,
                               Rarity rarity, ArtifactTag tags, ArtifactEffectType effectType, bool lockedByDefault = true)
        {
            this._artifactId = artifactId;
            this._displayName = displayName;
            this._description = description;
            this._rarity = rarity;
            this._tags = tags;
            this._effectType = effectType;
            this._lockedByDefault = lockedByDefault;
        }
#endif
    }
}
