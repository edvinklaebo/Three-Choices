using System.Collections.Generic;

using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(menuName = "Game/CharacterCollection")]
    public class CharacterCollection : ScriptableObject
    {
        [SerializeField]
        private List<CharacterDefinition> _characters;

        public IReadOnlyList<CharacterDefinition> Characters => this._characters;
    
        public CharacterDefinition GetByIndex(int index)
        {
            if (this._characters == null || this._characters.Count == 0)
                return null;

            return this._characters[Mathf.Clamp(index, 0, this._characters.Count - 1)];
        }
    }
}