using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashAsyncLoader : MonoBehaviour
{
    [Header("Scene Loading")] [SerializeField]
    private string nextScene = "MainMenu";

    [SerializeField] private Slider progressBar;
    [SerializeField] private float minDisplayTime = 1.5f;

    [Header("Logo Fade")] [SerializeField] private Image logo;

    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.4f;

    private IEnumerator Start()
    {
        // Fade logo in
        yield return Fade(logo, 0f, 1f, fadeInDuration);

        var startTime = Time.time;

        var op = SceneManager.LoadSceneAsync(nextScene);
        op!.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            progressBar.value = Mathf.Clamp01(op.progress / 0.9f);
            yield return null;
        }

        progressBar.value = 1f;

        var elapsed = Time.time - startTime;
        if (elapsed < minDisplayTime)
            yield return new WaitForSeconds(minDisplayTime - elapsed);

        // Fade out
        yield return Fade(logo, 1f, 0f, fadeOutDuration);

        // Destroy splash UI
        Destroy(gameObject);

        // Activate new scene
        op.allowSceneActivation = true;
    }

    private IEnumerator Fade(Image img, float from, float to, float duration)
    {
        var t = 0f;
        var c = img.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / duration);
            img.color = c;
            yield return null;
        }

        c.a = to;
        img.color = c;
    }
}