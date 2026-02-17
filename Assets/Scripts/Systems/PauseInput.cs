using UnityEngine;

/// <summary>
/// Handles input for pausing the game.
/// Separated from PauseManager to follow single responsibility principle.
/// </summary>
public class PauseInput : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseManager.TogglePause();
        }
    }
}
