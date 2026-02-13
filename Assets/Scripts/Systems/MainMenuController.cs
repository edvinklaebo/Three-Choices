using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Controls the main menu UI
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private RunController runController;

    private void Start()
    {
        runController = FindFirstObjectByType<RunController>();
        continueButton.gameObject.SetActive(SaveService.HasSave());
    }
    
    private void Awake()
    {
        quitButton.onClick.AddListener(OnQuitClicked);
        continueButton.onClick.AddListener(OnContinueClicked);
    }

    private static void OnQuitClicked()
    {
        Application.Quit();
    }

    private void OnContinueClicked()
    {
        if (runController != null)
        {
            runController.ContinueRun();
        }
        else
        {
            Log.Error("RunController not found when Continue was clicked");
        }
    }
}