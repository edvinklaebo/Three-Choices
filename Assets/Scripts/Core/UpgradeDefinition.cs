using Interfaces;

using UnityEngine;

namespace Core
{
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
        public string Id => this.id;
        public string DisplayName => this.displayName;
        public string Description => this.description;
        public Sprite Icon => this.icon;


        public Rarity GetRarity()
        {
            // Map rarityWeight values to Rarity enum
            // Values are mapped based on the Rarity enum values
            if (this.rarityWeight >= (int)Rarity.Common)
                return Rarity.Common;
            if (this.rarityWeight >= (int)Rarity.Uncommon)
                return Rarity.Uncommon;
            if (this.rarityWeight >= (int)Rarity.Rare)
                return Rarity.Rare;
            return Rarity.Epic;
        }

#if UNITY_EDITOR
        public void EditorInit(string identifier, string soName)
        {
            this.id = identifier;
            this.displayName = soName;
        }
#endif

        public abstract void Apply(Unit unit);
    }
}