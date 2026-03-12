using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Utils
{
    public class SplashAsyncLoader : MonoBehaviour
    {
        [Header("Scene Loading")]
        [SerializeField] private string nextScene = "MainMenu";
        [SerializeField] private Slider progressBar;
        [SerializeField] private float minDisplayTime = 1.5f;

        [Header("Logo Fade")]
        [SerializeField] private Image logo;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.4f;

        private void Start()
        {
            StartCoroutine(LoadRoutine());
        }

        private IEnumerator LoadRoutine()
        {
            if (this.logo)
                yield return Fade(this.logo, 0f, 1f, this.fadeInDuration);

            var startTime = Time.time;

            var op = SceneManager.LoadSceneAsync(this.nextScene);
            if (op == null)
                yield break;

            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                if (this.progressBar)
                    this.progressBar.value = Mathf.Clamp01(op.progress / 0.9f);

                yield return null;
            }

            if (this.progressBar)
                this.progressBar.value = 1f;

            var elapsed = Time.time - startTime;
            if (elapsed < this.minDisplayTime)
                yield return new WaitForSeconds(this.minDisplayTime - elapsed);

            if (this.logo)
                yield return Fade(this.logo, 1f, 0f, this.fadeOutDuration);

            op.allowSceneActivation = true;
        }

        private static IEnumerator Fade(Image img, float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                var color = img.color;
                color.a = to;
                img.color = color;
                yield break;
            }

            var t = 0f;
            var imgColor = img.color;

            while (t < duration)
            {
                t += Time.deltaTime;
                imgColor.a = Mathf.Lerp(from, to, t / duration);
                img.color = imgColor;
                yield return null;
            }

            imgColor.a = to;
            img.color = imgColor;
        }
    }
}