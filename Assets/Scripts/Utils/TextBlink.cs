using UnityEngine;
using TMPro;

public class TextBlink : MonoBehaviour
{
    private static readonly int FaceDilate = Shader.PropertyToID("_FaceDilate");
    [SerializeField] private TMP_Text text;
    [SerializeField] private float speed = 3f;   // How fast it pulses
    [SerializeField] private float minDilate;
    [SerializeField] private float maxDilate = 0.5f;

    private void Awake()
    {
        if (text == null)
            text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (text == null) 
            return;

        // Pulse the dilate property with a sine wave
        var dilate = Mathf.Lerp(minDilate, maxDilate, (Mathf.Sin(Time.time * speed) + 1f) / 2f);

        text.fontMaterial.SetFloat(FaceDilate, dilate);
    }
}