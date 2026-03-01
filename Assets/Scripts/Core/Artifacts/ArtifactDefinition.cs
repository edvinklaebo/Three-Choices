using UnityEngine;

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
    public bool LockedByDefault => _lockedByDefault;

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
#endif
}
