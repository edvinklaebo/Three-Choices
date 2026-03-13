using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(menuName = "Game/Character")]
    public class CharacterDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public int MaxHp;
        public int Attack;
        public int Armor;
        public int Speed;
        public int Mana;
        public int MagicScaling;
        public int HealingScaling;
        public int StatusScaling;
        public Sprite Portrait;
    }
}