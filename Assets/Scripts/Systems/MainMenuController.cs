using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Controls the main menu UI
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button continueButton;

    private void Start()
    {
        continueButton.gameObject.SetActive(SaveService.HasSave());
    }
    
    private void Awake()
    {
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    private static void OnQuitClicked()
    {
        Application.Quit();
    }
}