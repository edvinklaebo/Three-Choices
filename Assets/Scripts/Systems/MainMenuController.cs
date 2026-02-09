using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Controls the main menu UI
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnPlayClicked()
    {
        SceneManager.LoadScene("DraftScene"); // or your game scene
    }

    private void OnSettingsClicked()
    {
        // open settings panel
    }

    private void OnQuitClicked()
    {
        Application.Quit();
    }
}