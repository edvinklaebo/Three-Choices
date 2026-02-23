using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class CharacterSelectionTests
    {
        private CharacterCollection _testCollection;

        [SetUp]
        public void Setup()
        {
            _testCollection = ScriptableObject.CreateInstance<CharacterCollection>();

            var characters = new List<CharacterDefinition>();

            for (var i = 0; i < 3; i++)
            {
                var character = ScriptableObject.CreateInstance<CharacterDefinition>();
                character.Id = $"char_{i}";
                character.DisplayName = $"Character {i}";
                character.MaxHp = 100 + i * 10;
                character.Attack = 10 + i;
                character.Armor = 5 + i;
                character.Speed = 10;

                characters.Add(character);
            }

            var field = typeof(CharacterCollection)
                .GetField("_characters", BindingFlags.NonPublic | BindingFlags.Instance);

            field?.SetValue(_testCollection, characters);
        }

        [TearDown]
        public void Cleanup()
        {
            if (_testCollection != null) Object.DestroyImmediate(_testCollection);
        }

        [Test]
        public void CharacterCollection_HasThreeCharacters()
        {
            // Assert
            Assert.AreEqual(3, _testCollection.Characters.Count);
        }

        [Test]
        public void CharacterCollection_GetByIndex_ReturnsCorrectCharacter()
        {
            // Act
            var character = _testCollection.GetByIndex(1);

            // Assert
            Assert.NotNull(character);
            Assert.AreEqual("char_1", character.Id);
        }

        [Test]
        public void CharacterCollection_GetByIndex_ClampsToValidRange()
        {
            // Act - Try to get index beyond range
            var character = _testCollection.GetByIndex(10);

            // Assert - Should clamp to last valid index
            Assert.NotNull(character);
            Assert.AreEqual("char_2", character.Id);
        }

        [Test]
        public void CharacterCollection_GetByIndex_ClampsNegativeToZero()
        {
            // Act
            var character = _testCollection.GetByIndex(-1);

            // Assert - Should clamp to 0
            Assert.NotNull(character);
            Assert.AreEqual("char_0", character.Id);
        }

        [Test]
        public void CharacterSelectionModel_Next_WrapsAround()
        {
            // Arrange
            var model = new CharacterSelectionModel(_testCollection.Characters);

            // Act
            model.Next(); // Index 1
            model.Next(); // Index 2
            model.Next(); // Should wrap to 0

            // Assert
            Assert.AreEqual(0, model.CurrentIndex);
        }

        [Test]
        public void CharacterSelectionModel_Previous_WrapsAround()
        {
            // Arrange
            var model = new CharacterSelectionModel(_testCollection.Characters);

            // Act - Previous from index 0 should wrap to last
            model.Previous();

            // Assert
            Assert.AreEqual(2, model.CurrentIndex);
        }

        [Test]
        public void CharacterSelectionModel_Current_ReturnsCorrectCharacter()
        {
            // Arrange
            var model = new CharacterSelectionModel(_testCollection.Characters);

            // Act
            model.Next(); // Move to index 1

            // Assert
            Assert.AreEqual("char_1", model.Current.Id);
        }

        [Test]
        public void CharacterSelectController_Confirm_FiresCharacterSelectedEvent()
        {
            // Arrange
            var go = new GameObject();
            var controller = go.AddComponent<CharacterSelectController>();

            var model = new CharacterSelectionModel(_testCollection.Characters);
            var modelField = typeof(CharacterSelectController).GetField("_model",
                BindingFlags.NonPublic | BindingFlags.Instance);
            modelField?.SetValue(controller, model);

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