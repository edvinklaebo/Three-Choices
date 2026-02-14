using System;

/// <summary>
///     Static events for character selection flow.
///     NOTE: Subscribers must clean up in OnDisable/OnDestroy to prevent memory leaks.
/// </summary>
public static class GameEvents
{
    public static Action NewRunRequested_Event;
    public static Action<CharacterDefinition> CharacterSelected_Event;

    /// <summary>
    ///     Clear all event subscribers. Call during scene cleanup if needed.
    /// </summary>
    public static void ClearAll()
    {
        NewRunRequested_Event = null;
        CharacterSelected_Event = null;
    }
}