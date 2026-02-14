using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class CharacterSelectionTests
    {
        private CharacterDatabase _testDatabase;

        [SetUp]
        public void Setup()
        {
            _testDatabase = ScriptableObject.CreateInstance<CharacterDatabase>();
            _testDatabase.Characters = new System.Collections.Generic.List<CharacterDefinition>();

            // Create 3 test characters
            for (int i = 0; i < 3; i++)
            {
                var character = ScriptableObject.CreateInstance<CharacterDefinition>();
                character.Id = $"char_{i}";
                character.DisplayName = $"Character {i}";
                character.MaxHp = 100 + i * 10;
                character.Attack = 10 + i;
                character.Armor = 5 + i;
                character.Speed = 10;
                _testDatabase.Characters.Add(character);
            }
        }

        [TearDown]
        public void Cleanup()
        {
            if (_testDatabase != null)
            {
                Object.DestroyImmediate(_testDatabase);
            }
        }

        [Test]
        public void Database_HasThreeCharacters()
        {
            // Assert
            Assert.AreEqual(3, _testDatabase.Characters.Count);
        }

        [Test]
        public void Database_GetByIndex_ReturnsCorrectCharacter()
        {
            // Act
            var character = _testDatabase.GetByIndex(1);

            // Assert
            Assert.NotNull(character);
            Assert.AreEqual("char_1", character.Id);
        }

        [Test]
        public void Database_GetByIndex_ClampsToValidRange()
        {
            // Act - Try to get index beyond range
            var character = _testDatabase.GetByIndex(10);

            // Assert - Should clamp to last valid index
            Assert.NotNull(character);
            Assert.AreEqual("char_2", character.Id);
        }

        [Test]
        public void Database_GetByIndex_ClampsNegativeToZero()
        {
            // Act
            var character = _testDatabase.GetByIndex(-1);

            // Assert - Should clamp to 0
            Assert.NotNull(character);
            Assert.AreEqual("char_0", character.Id);
        }

        [Test]
        public void Controller_Next_WrapsAround()
        {
            // Arrange
            var go = new GameObject();
            var controller = go.AddComponent<CharacterSelectController>();
            
            // Use reflection to set the private _database field
            var databaseField = typeof(CharacterSelectController).GetField("_database", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            databaseField.SetValue(controller, _testDatabase);

            // Act
            controller.Next(); // Index 1
            controller.Next(); // Index 2
            controller.Next(); // Should wrap to 0

            // Assert
            Assert.AreEqual(0, controller.CurrentIndex);
            
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Controller_Previous_WrapsAround()
        {
            // Arrange
            var go = new GameObject();
            var controller = go.AddComponent<CharacterSelectController>();
            
            var databaseField = typeof(CharacterSelectController).GetField("_database", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            databaseField.SetValue(controller, _testDatabase);

            // Act - Previous from index 0 should wrap to last
            controller.Previous();

            // Assert
            Assert.AreEqual(2, controller.CurrentIndex);
            
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Controller_Confirm_FiresCharacterSelectedEvent()
        {
            // Arrange
            var go = new GameObject();
            var controller = go.AddComponent<CharacterSelectController>();
            
            var databaseField = typeof(CharacterSelectController).GetField("_database", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            databaseField.SetValue(controller, _testDatabase);

            CharacterDefinition received = null;
            GameEvents.CharacterSelected_Event += c => received = c;

            // Act
            controller.Next(); // Move to character 1
            controller.Confirm();

            // Assert
            Assert.NotNull(received);
            Assert.AreEqual("char_1", received.Id);

            // Cleanup
            GameEvents.CharacterSelected_Event = null;
            Object.DestroyImmediate(go);
        }

        [Test]
        public void CharacterDefinition_HasRequiredProperties()
        {
            // Arrange
            var character = ScriptableObject.CreateInstance<CharacterDefinition>();
            character.Id = "test";
            character.DisplayName = "Test Hero";
            character.MaxHp = 100;
            character.Attack = 10;
            character.Armor = 5;
            character.Speed = 12;

            // Assert
            Assert.AreEqual("test", character.Id);
            Assert.AreEqual("Test Hero", character.DisplayName);
            Assert.AreEqual(100, character.MaxHp);
            Assert.AreEqual(10, character.Attack);
            Assert.AreEqual(5, character.Armor);
            Assert.AreEqual(12, character.Speed);

            Object.DestroyImmediate(character);
        }
    }
}
