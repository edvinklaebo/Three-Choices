using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Controls the main menu UI
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button continueButton;
    
    private RunController runController;

    private void Awake()
    {
        runController = FindFirstObjectByType<RunController>();
        quitButton.onClick.AddListener(OnQuitClicked);
        continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void Start()
    {
        continueButton.gameObject.SetActive(SaveService.HasSave());
    }

    private static void OnQuitClicked()
    {
        Application.Quit();
    }

    public void OnContinueClicked()
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