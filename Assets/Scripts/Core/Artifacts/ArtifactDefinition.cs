using Core.StatusEffects;
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

        [Header("Status Effect Data (Optional)")]
        [Tooltip("Balance data for Bleed-based artifacts (e.g. BloodRitual). Leave empty to use code defaults.")]
        [SerializeField] private BleedDefinition _bleedDefinition;

        [Tooltip("Balance data for Poison-based artifacts (e.g. PoisonAmplifier). Leave empty to use code defaults.")]
        [SerializeField] private PoisonDefinition _poisonDefinition;

        [Tooltip("Balance data for Burn-based artifacts (e.g. BlazingTorch). Leave empty to use code defaults.")]
        [SerializeField] private BurnDefinition _burnDefinition;

        [Header("Meta-Progression")]
        [SerializeField] private bool _lockedByDefault = true;

        // ---- Public read-only accessors ----
        public string Id => _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public Rarity Rarity => _rarity;
        public Rarity GetRarity() => _rarity;
        public ArtifactTag Tags => _tags;
        public ArtifactEffectType EffectType => _effectType;
        public ArtifactId ArtifactId => _artifactId;
        public bool LockedByDefault => _lockedByDefault;
        public BleedDefinition BleedDefinition => _bleedDefinition;
        public PoisonDefinition PoisonDefinition => _poisonDefinition;
        public BurnDefinition BurnDefinition => _burnDefinition;

#if UNITY_EDITOR
        public void EditorInit(string id, string displayName, string description,
                               Rarity rarity, ArtifactTag tags, ArtifactEffectType effectType, bool lockedByDefault = true)
        {
            _id = id;
            _displayName = displayName;
            _description = description;
            _rarity = rarity;
            _tags = tags;
            _effectType = effectType;
            _lockedByDefault = lockedByDefault;
        }

        public void EditorInit(ArtifactId artifactId, string displayName, string description,
                               Rarity rarity, ArtifactTag tags, ArtifactEffectType effectType, bool lockedByDefault = true)
        {
            _artifactId = artifactId;
            _displayName = displayName;
            _description = description;
            _rarity = rarity;
            _tags = tags;
            _effectType = effectType;
            _lockedByDefault = lockedByDefault;
        }

        public void EditorInitStatusEffectData(BleedDefinition bleedDefinition = null, PoisonDefinition poisonDefinition = null, BurnDefinition burnDefinition = null)
        {
            _bleedDefinition = bleedDefinition;
            _poisonDefinition = poisonDefinition;
            _burnDefinition = burnDefinition;
        }
#endif
    }
}
