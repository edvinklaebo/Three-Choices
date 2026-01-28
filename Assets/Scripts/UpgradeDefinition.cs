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
    public string Id;
    public string DisplayName;
    [TextArea] public string Description;

    public UpgradeType Type;

    // Stat upgrades
    public StatType Stat;
    public int Amount;

    // Ability / passive id (string for now, pragmatic)
    public string AbilityId;

    public int RarityWeight = 100;
}