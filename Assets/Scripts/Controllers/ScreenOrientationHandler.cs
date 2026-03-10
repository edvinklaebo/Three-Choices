using UnityEngine;

/// <summary>
/// Enforces landscape-only orientation at runtime so the game is never displayed
/// in portrait mode on mobile devices. Persists across scene loads.
/// </summary>
public class ScreenOrientationHandler : MonoBehaviour
{
    private static ScreenOrientationHandler _instance;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
}
