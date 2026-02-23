using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/CharacterCollection")]
public class CharacterCollection : ScriptableObject
{
    [SerializeField]
    private List<CharacterDefinition> _characters;

    public IReadOnlyList<CharacterDefinition> Characters => _characters;
    
    public CharacterDefinition GetByIndex(int index)
    {
        if (Characters == null || Characters.Count == 0)
            return null;

        return _characters[Mathf.Clamp(index, 0, _characters.Count - 1)];
    }
}