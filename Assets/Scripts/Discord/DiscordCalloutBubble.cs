using System.Collections;

using UnityEngine;

namespace Discord
{
    // Displays a short "Join our discord!" speech bubble animation near the Discord button.
    // Call PlayAnimation() to trigger a fade-in, hold, then fade-out sequence.
    public class DiscordCalloutBubble : MonoBehaviour
    {
        private const float FadeDuration = 0.4f;

        [SerializeField] private float _holdDuration = 3f;

        private CanvasGroup _canvasGroup;
        private Coroutine _activeRoutine;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            Debug.Assert(_canvasGroup != null, "DiscordCalloutBubble requires a CanvasGroup component", this);
            Debug.Assert(_holdDuration > 0f, "DiscordCalloutBubble: _holdDuration must be greater than 0", this);
            Hide();
        }

        public void PlayAnimation()
        {
            if (_activeRoutine != null)
                StopCoroutine(_activeRoutine);

            _activeRoutine = StartCoroutine(AnimationRoutine());
        }

        private void Hide()
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private IEnumerator AnimationRoutine()
        {
            yield return FadeAlpha(0f, 1f, FadeDuration);
            yield return new WaitForSeconds(_holdDuration);
            yield return FadeAlpha(1f, 0f, FadeDuration);
            Hide();
            _activeRoutine = null;
        }

        private IEnumerator FadeAlpha(float from, float to, float duration)
        {
            var elapsed = 0f;
            _canvasGroup.alpha = from;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            _canvasGroup.alpha = to;
        }
    }
}
