using UnityEngine;

public enum StatType
{
    MaxHP,
    AttackPower,
    Armor,
    Speed
}

public abstract class UpgradeDefinition : ScriptableObject, IDraftable
{
    [Header("Identity")] 
    [SerializeField] protected string id;
    [SerializeField] protected string displayName;
    
    [TextArea] 
    [SerializeField] protected string description;
    [SerializeField] protected Sprite icon;

    [Header("Draft / Balance")] 
    [Min(1)] 
    [SerializeField]
    protected int rarityWeight = 100;

    
    // ---- Public read-only accessors ----
    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;


    public Rarity GetRarity()
    {
        // Map rarityWeight values to Rarity enum
        // Values are mapped based on the Rarity enum values
        if (rarityWeight >= (int)Rarity.Common)
            return Rarity.Common;
        if (rarityWeight >= (int)Rarity.Uncommon)
            return Rarity.Uncommon;
        if (rarityWeight >= (int)Rarity.Rare)
            return Rarity.Rare;
        return Rarity.Epic;
    }

#if UNITY_EDITOR
    public void EditorInit(string identifier, string soName)
    {
        id = identifier;
        displayName = soName;
    }
#endif

    public abstract void Apply(Unit unit);
}