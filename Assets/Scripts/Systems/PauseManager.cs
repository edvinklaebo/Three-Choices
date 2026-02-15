using System;
using UnityEngine;

/// <summary>
/// Static pause manager handling pause state, time scale, and cursor visibility.
/// Follows single responsibility principle - only manages pause state and time.
/// </summary>
public static class PauseManager
{
    public static bool IsPaused { get; private set; }

    public static event Action<bool> OnPauseStateChanged;

    public static void Pause()
    {
        if (IsPaused) return;

        IsPaused = true;
        Time.timeScale = 0f;
        OnPauseStateChanged?.Invoke(true);
    }

    public static void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;
        OnPauseStateChanged?.Invoke(false);
    }

    public static void TogglePause()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }
}
