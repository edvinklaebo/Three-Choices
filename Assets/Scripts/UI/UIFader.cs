using System.Collections;
using UnityEngine;

public class UIFader
{
    private readonly float _fadeDuration;
    private readonly CanvasGroup _canvasGroup;
    private readonly MonoBehaviour _coroutineRunner;

    public UIFader(CanvasGroup canvasGroup, MonoBehaviour coroutineRunner, float fadeDuration = 0.3f)
    {
        _canvasGroup = canvasGroup;
        _coroutineRunner = coroutineRunner;
        _fadeDuration = fadeDuration;
    }

    public void Show(bool animated = true)
    {
        if (animated)
        {
            _coroutineRunner.StartCoroutine(FadeIn());
        }
        else
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }

    public void Hide(bool animated = true)
    {
        if (animated)
        {
            _coroutineRunner.StartCoroutine(FadeOut());
        }
        else
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    private IEnumerator FadeIn()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        float elapsed = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / _fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    private IEnumerator FadeOut()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        float elapsed = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / _fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
    }
}
