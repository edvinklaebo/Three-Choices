using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;

using Utils;

namespace Intro
{
    public class IntroController : MonoBehaviour
    {
        private const string MainMenuSceneName = "MainMenu";
        private const float FadeDuration = 1f;

        [Header("UI References")]
        [SerializeField] private TMP_Text _narrativeText;
        [SerializeField] private TMP_Text _skipHintText;

        [Header("Timing")]
        [SerializeField] private float _lineDelay = 3f;

        private IntroSequence _sequence;
        private bool _skipping;

        private void Start()
        {
            if (this._narrativeText == null)
            {
                Log.Error("IntroController: _narrativeText is not assigned.");
                return;
            }

            if (this._skipHintText == null)
                Log.Warning("IntroController: _skipHintText is not assigned.");

            this._sequence = new IntroSequence(IntroSequence.DefaultLines);
            this._sequence.OnLineShown += UpdateText;

            SetTextAlpha(0f);
            StartCoroutine(PlaySequence());
        }

        private void Update()
        {
            if (this._sequence != null && !this._skipping && Input.anyKeyDown)
                SkipIntro();
        }

        private IEnumerator PlaySequence()
        {
            while (!this._sequence.IsComplete)
            {
                this._sequence.ShowNext();
                yield return FadeText(0f, 1f);
                yield return new WaitForSeconds(this._lineDelay);
                yield return FadeText(1f, 0f);
            }

            LoadMainMenu();
        }

        private void UpdateText(string line)
        {
            if (this._narrativeText != null)
                this._narrativeText.text = line;
        }

        private void SkipIntro()
        {
            this._skipping = true;
            StopAllCoroutines();
            LoadMainMenu();
        }
    
        private void SetTextAlpha(float alpha)
        {
            var color = this._narrativeText.color;
            color.a = alpha;
            this._narrativeText.color = color;
        }

        private IEnumerator FadeText(float from, float to)
        {
            var t = 0f;
            while (t < FadeDuration)
            {
                t += Time.deltaTime;
                SetTextAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(t / FadeDuration)));
                yield return null;
            }
            SetTextAlpha(to);
        }

        private static void LoadMainMenu()
        {
            Debug.Assert(!string.IsNullOrEmpty(MainMenuSceneName), "IntroController: MainMenuSceneName is not set.");
            SceneManager.LoadScene(MainMenuSceneName);
        }
    }
}
