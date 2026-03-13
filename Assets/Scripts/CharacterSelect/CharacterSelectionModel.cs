using System;
using System.Collections.Generic;

using Characters;

namespace CharacterSelect
{
    public class CharacterSelectionModel
    {
        private readonly IReadOnlyList<CharacterDefinition> _characters;

        public int CurrentIndex { get; private set; }
        public CharacterDefinition Current => this._characters[CurrentIndex];

        public CharacterSelectionModel(IReadOnlyList<CharacterDefinition> characters)
        {
            if (characters == null || characters.Count == 0)
                throw new ArgumentException("Characters list cannot be null or empty.", nameof(characters));

            this._characters = characters;
        }

        public void Next()
        {
            CurrentIndex = (CurrentIndex + 1) % this._characters.Count;
        }

        public void Previous()
        {
            CurrentIndex = (CurrentIndex - 1 + this._characters.Count) % this._characters.Count;
        }
    }
}
