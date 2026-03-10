using UnityEngine;

/// <summary>
/// Utility helpers for platform-specific behaviour.
/// </summary>
public static class PlatformUtils
{
    /// <summary>
    /// Returns true when the running platform supports an explicit quit action
    /// (standalone Windows, Linux and macOS builds, plus the Unity Editor).
    /// On mobile, console, and web platforms the OS manages application
    /// lifecycle, so a quit button should be hidden.
    /// </summary>
    public static bool IsQuitSupported()
    {
#if UNITY_EDITOR
        return true;
#else
        return IsQuitSupportedOnPlatform(Application.platform);
#endif
    }

    /// <summary>
    /// Platform-agnostic core check used for unit testing.
    /// </summary>
    public static bool IsQuitSupportedOnPlatform(RuntimePlatform platform)
    {
        return platform == RuntimePlatform.WindowsPlayer
            || platform == RuntimePlatform.LinuxPlayer
            || platform == RuntimePlatform.OSXPlayer;
    }
}
