using System;
using System.Collections.Generic;

public class CharacterSelectionModel
{
    private readonly IReadOnlyList<CharacterDefinition> _characters;

    public int CurrentIndex { get; private set; }
    public CharacterDefinition Current => _characters[CurrentIndex];

    public CharacterSelectionModel(IReadOnlyList<CharacterDefinition> characters)
    {
        if (characters == null || characters.Count == 0)
            throw new ArgumentException("Characters list cannot be null or empty.", nameof(characters));

        _characters = characters;
    }

    public void Next()
    {
        CurrentIndex = (CurrentIndex + 1) % _characters.Count;
    }

    public void Previous()
    {
        CurrentIndex = (CurrentIndex - 1 + _characters.Count) % _characters.Count;
    }
}
