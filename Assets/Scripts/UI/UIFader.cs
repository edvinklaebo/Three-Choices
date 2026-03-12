using System.Collections;

using UnityEngine;

namespace UI
{
    public class UIFader
    {
        private readonly float _fadeDuration;
        private readonly CanvasGroup _canvasGroup;
        private readonly MonoBehaviour _coroutineRunner;

        public UIFader(CanvasGroup canvasGroup, MonoBehaviour coroutineRunner, float fadeDuration = 0.3f)
        {
            this._canvasGroup = canvasGroup;
            this._coroutineRunner = coroutineRunner;
            this._fadeDuration = fadeDuration;
        }

        public void Show(bool animated = true)
        {
            if (animated)
            {
                this._coroutineRunner.StartCoroutine(FadeIn());
            }
            else
            {
                this._canvasGroup.alpha = 1f;
                this._canvasGroup.interactable = true;
                this._canvasGroup.blocksRaycasts = true;
            }
        }

        public void Hide(bool animated = true)
        {
            if (animated)
            {
                this._coroutineRunner.StartCoroutine(FadeOut());
            }
            else
            {
                this._canvasGroup.alpha = 0f;
                this._canvasGroup.interactable = false;
                this._canvasGroup.blocksRaycasts = false;
            }
        }

        private IEnumerator FadeIn()
        {
            this._canvasGroup.alpha = 0f;
            this._canvasGroup.interactable = false;
            this._canvasGroup.blocksRaycasts = false;

            float elapsed = 0f;

            while (elapsed < this._fadeDuration)
            {
                elapsed += Time.deltaTime;
                this._canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / this._fadeDuration);
                yield return null;
            }

            this._canvasGroup.alpha = 1f;
            this._canvasGroup.interactable = true;
            this._canvasGroup.blocksRaycasts = true;
        }

        private IEnumerator FadeOut()
        {
            this._canvasGroup.interactable = false;
            this._canvasGroup.blocksRaycasts = false;

            float elapsed = 0f;

            while (elapsed < this._fadeDuration)
            {
                elapsed += Time.deltaTime;
                this._canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / this._fadeDuration);
                yield return null;
            }

            this._canvasGroup.alpha = 0f;
        }
    }
}
