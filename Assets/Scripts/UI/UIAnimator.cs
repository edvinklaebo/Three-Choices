using System.Collections;
using UnityEngine;

public static class UIAnimator
{
    public static Coroutine AnimateScale(Transform t, Vector3 from, Vector3 to, float duration, MonoBehaviour runner)
    {
        return runner.StartCoroutine(ScaleRoutine(t, from, to, duration));
    }

    private static IEnumerator ScaleRoutine(Transform t, Vector3 from, Vector3 to, float duration)
    {
        if (duration <= 0f)
        {
            t.localScale = to;
            yield break;
        }

        var elapsed = 0f;
        t.localScale = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var progress = Mathf.Clamp01(elapsed / duration);
            progress = 1f - Mathf.Pow(1f - progress, 3f);
            t.localScale = Vector3.Lerp(from, to, progress);
            yield return null;
        }

        t.localScale = to;
    }
}
