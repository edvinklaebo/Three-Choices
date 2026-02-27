using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";

    [Header("UI References")]
    [SerializeField] private TMP_Text _narrativeText;
    [SerializeField] private TMP_Text _skipHintText;

    [Header("Timing")]
    [SerializeField] private float _lineDelay = 3f;

    private IntroSequence _sequence;
    private bool _skipping;

    private void Start()
    {
        if (_narrativeText == null)
        {
            Log.Error("IntroController: _narrativeText is not assigned.");
            return;
        }

        if (_skipHintText == null)
            Log.Warning("IntroController: _skipHintText is not assigned.");

        _sequence = new IntroSequence(IntroSequence.DefaultLines);
        _sequence.OnLineShown += UpdateText;
        _sequence.OnComplete += LoadMainMenu;

        _narrativeText.text = string.Empty;

        StartCoroutine(PlaySequence());
    }

    private void Update()
    {
        if (_sequence != null && !_skipping && Input.anyKeyDown)
            SkipIntro();
    }

    private IEnumerator PlaySequence()
    {
        while (!_sequence.IsComplete)
        {
            _sequence.ShowNext();

            if (!_sequence.IsComplete)
                yield return new WaitForSeconds(_lineDelay);
        }
    }

    private void UpdateText(string line)
    {
        if (_narrativeText != null)
            _narrativeText.text = line;
    }

    private void SkipIntro()
    {
        _skipping = true;
        StopAllCoroutines();
        _sequence.Skip();
    }

    private static void LoadMainMenu()
    {
        Debug.Assert(!string.IsNullOrEmpty(MainMenuSceneName), "IntroController: MainMenuSceneName is not set.");
        SceneManager.LoadScene(MainMenuSceneName);
    }
}
