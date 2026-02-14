using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    public List<CharacterDefinition> Characters;

    public CharacterDefinition GetByIndex(int index)
    {
        if (Characters == null || Characters.Count == 0)
            return null;

        return Characters[Mathf.Clamp(index, 0, Characters.Count - 1)];
    }
}