using UnityEngine;

[CreateAssetMenu(menuName = "Game/Character")]
public class CharacterDefinition : ScriptableObject
{
    public string Id;
    public string DisplayName;
    public int MaxHp;
    public int Attack;
    public int Armor;
    public int Speed;
    public Sprite Portrait;
}