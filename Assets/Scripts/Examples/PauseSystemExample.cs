using UnityEngine;

/// <summary>
/// Example integration showing how to add pause functionality to a scene.
/// This demonstrates the simplest way to enable the pause system.
/// 
/// Usage:
/// 1. Attach this script to any GameObject in your scene
/// 2. That's it! The pause menu will be created and Esc key will work.
/// 
/// For more control, you can:
/// - Disable createUI and build your own UI in the Unity Editor
/// - Subscribe to PauseManager.OnPauseStateChanged to react to pause events
/// - Check PauseManager.IsPaused in your game logic
/// </summary>
public class PauseSystemExample : MonoBehaviour
{
    [Tooltip("If true, creates pause menu UI automatically at runtime")]
    [SerializeField] private bool createUI = true;

    private void Start()
    {
        // Option 1: Use the bootstrap to create UI automatically
        if (createUI)
        {
            gameObject.AddComponent<PauseMenuBootstrap>();
        }
        // Option 2: Set up UI manually in Unity Editor and just add PauseInput
        else
        {
            // Assumes you've already created the UI in the scene
            // Just need the input handler
            if (FindFirstObjectByType<PauseInput>() == null)
            {
                gameObject.AddComponent<PauseInput>();
            }
        }

        // Optional: Subscribe to pause events if you need to react
        PauseManager.OnPauseStateChanged += OnPauseChanged;
    }

    private void OnDestroy()
    {
        // Always unsubscribe from events
        PauseManager.OnPauseStateChanged -= OnPauseChanged;
    }

    private void OnPauseChanged(bool isPaused)
    {
        // React to pause state changes in your game logic
        if (isPaused)
        {
            Debug.Log("Game paused - you could disable certain systems here");
            // Example: Stop background music, pause AI, etc.
        }
        else
        {
            Debug.Log("Game resumed - re-enable systems here");
            // Example: Resume background music, resume AI, etc.
        }
    }

    // Example: Check pause state in game logic
    private void Update()
    {
        // Skip gameplay logic when paused
        if (PauseManager.IsPaused)
            return;

        // Your gameplay logic here...
        // This won't execute while paused
    }
}
