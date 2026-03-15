using Events;

using Systems;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Utils;

// Controls the main menu UI
namespace Controllers
{
    public class MainMenuController : MonoBehaviour
    {
        private const string CharacterSelectSceneName = "CharacterSelectScene";

        [Header("Events")] 
        [SerializeField] private VoidEventChannel _continueRunRequested;

        [Header("References")] 
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;


        private void Awake()
        {
            quitButton.onClick.AddListener(OnQuitClicked);
            continueButton.onClick.AddListener(OnContinueClicked);

            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGamePressed);
        }

        private void Start()
        {
            continueButton.gameObject.SetActive(SaveService.HasSave());
            quitButton.gameObject.SetActive(PlatformUtils.IsQuitSupported());
        }

        private static void OnQuitClicked()
        {
            Application.Quit();
        }

        private void OnContinueClicked()
        {
            if (_continueRunRequested != null)
                _continueRunRequested.Raise();
            else
                Log.Error("MainMenuController: ContinueRunRequested event channel not assigned");
        }

        private static void OnNewGamePressed()
        {
            Log.Info("[MainMenu] New run requested.");
            GameEvents.NewRunRequested_Event?.Invoke();
            SceneManager.LoadScene(CharacterSelectSceneName);
        }
    }
}