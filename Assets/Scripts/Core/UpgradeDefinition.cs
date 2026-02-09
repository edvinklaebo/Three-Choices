using UnityEngine;

public enum StatType
{
    MaxHP,
    AttackPower,
    Armor,
    Speed
}

[CreateAssetMenu(menuName = "Upgrades/Upgrade")]
public class UpgradeDefinition : ScriptableObject
{
    [Header("Identity")] [SerializeField] private string id;
    [SerializeField] private string displayName;
    [TextArea] [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    [Header("Type")] [SerializeField] private UpgradeType type;

    [Header("Stat Upgrade")] [SerializeField] private StatType stat;
    [SerializeField] private int amount;

    [Header("Ability Upgrade")] [SerializeField] private string abilityId;

    [Header("Draft / Balance")] [Min(1)] [SerializeField] private int rarityWeight = 100;

    // ---- Public read-only accessors ----
    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;

    public UpgradeType Type => type;
    public StatType Stat => stat;
    public int Amount => amount;
    public string AbilityId => abilityId;

    public int RarityWeight => rarityWeight;
    
#if UNITY_EDITOR
    public void EditorInit(string identifier, string soName, UpgradeType soType, StatType soStat, int soAmount)
    {
        id = identifier;
        displayName = soName;
        type = soType;
        stat = soStat;
        amount = soAmount;
    }
    
    public void EditorInit(string identifier, string soName, UpgradeType soType, string soAbilityId)
    {
        id = identifier;
        displayName = soName;
        type = soType;
        abilityId = soAbilityId;
    }
    
    public void EditorInit(string identifier, string soName)
    {
        id = identifier;
        displayName = soName;
    }
#endif
}