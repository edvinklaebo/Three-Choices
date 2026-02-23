using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class PlayerFactoryTests
    {
        [Test]
        public void CreateFromCharacter_ReturnsUnitWithCorrectStats()
        {
            var character = ScriptableObject.CreateInstance<CharacterDefinition>();
            character.DisplayName = "Warrior";
            character.MaxHp = 120;
            character.Attack = 14;
            character.Armor = 6;
            character.Speed = 8;

            var player = PlayerFactory.CreateFromCharacter(character);

            Assert.IsNotNull(player);
            Assert.AreEqual("Warrior", player.Name);
            Assert.AreEqual(120, player.Stats.MaxHP);
            Assert.AreEqual(120, player.Stats.CurrentHP);
            Assert.AreEqual(14, player.Stats.AttackPower);
            Assert.AreEqual(6, player.Stats.Armor);
            Assert.AreEqual(8, player.Stats.Speed);

            Object.DestroyImmediate(character);
        }

        [Test]
        public void CreateFromCharacter_WithNullCharacter_ReturnsNull()
        {
            var player = PlayerFactory.CreateFromCharacter(null);

            Assert.IsNull(player);
        }
    }
}
