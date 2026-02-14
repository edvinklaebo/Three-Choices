using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeOverlay : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    ///     Fade the overlay from current alpha to target alpha over duration.
    /// </summary>
    public void FadeTo(float targetAlpha, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeCoroutine(targetAlpha, duration));
    }

    /// <summary>
    ///     Fade in (from black to transparent)
    /// </summary>
    public void FadeIn(float duration = 0.5f)
    {
        FadeTo(0f, duration);
    }

    /// <summary>
    ///     Fade out (from transparent to black)
    /// </summary>
    public void FadeOut(float duration = 0.5f)
    {
        FadeTo(1f, duration);
    }

    private IEnumerator FadeCoroutine(float targetAlpha, float duration)
    {
        var startAlpha = canvasGroup.alpha;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}