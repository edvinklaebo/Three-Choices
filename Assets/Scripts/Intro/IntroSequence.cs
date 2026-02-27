using System;

public class IntroSequence
{
    public static readonly string[] DefaultLines =
    {
        "The world once feared necromancers.",
        "Kings burned them.\nPriests hunted them.\nCities drove them into the wastelands.",
        "But death is patient.",
        "And the dead are many.",
        "In time, the necromancers returned.",
        "Not as conquerors.",
        "As entertainers.",
        "They built arenas from bone and stone.",
        "There, disputes are settled the old way.",
        "Steel.\nMagic.\nAnd the dead.",
        "Each necromancer brings a champion.",
        "A warrior rebuilt from the remains of the fallen.",
        "The strongest rise through the tournament.",
        "The weak are broken apart...",
        "And used again.",
        "You are no champion.",
        "Not yet.",
        "You are only a skull\npulled from a pile of forgotten bones.",
        "But even the smallest fragment of death\ncan become a weapon.",
        "Win your battles.",
        "Grow stronger.",
        "Climb the arena.",
        "And perhaps...",
        "One day...",
        "You will sit upon the Bone Throne."
    };

    private readonly string[] _lines;
    private int _currentIndex;

    public event Action<string> OnLineShown;
    public event Action OnComplete;

    public int CurrentIndex => _currentIndex;
    public int TotalLines => _lines.Length;
    public bool IsComplete => _currentIndex >= _lines.Length;

    public IntroSequence(string[] lines)
    {
        if (lines == null || lines.Length == 0)
            throw new ArgumentException("Lines cannot be null or empty.", nameof(lines));

        _lines = lines;
    }

    public void ShowNext()
    {
        if (IsComplete)
            return;

        OnLineShown?.Invoke(_lines[_currentIndex]);
        _currentIndex++;

        if (IsComplete)
            OnComplete?.Invoke();
    }

    public void Skip()
    {
        if (IsComplete)
            return;

        _currentIndex = _lines.Length;
        OnComplete?.Invoke();
    }
}
