using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TMPTextPulse : MonoBehaviour
{
    private static readonly int FaceDilate = Shader.PropertyToID("_FaceDilate");

    [SerializeField] private float speed = 3f;
    [SerializeField] private float minDilate;
    [SerializeField] private float maxDilate = 0.5f;

    private TMP_Text text;
    private Material material;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        material = text.fontMaterial;
    }

    private void Update()
    {
        var t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
        var dilate = Mathf.Lerp(minDilate, maxDilate, t);

        material.SetFloat(FaceDilate, dilate);
    }

    private void OnValidate()
    {
        if (maxDilate < minDilate)
            maxDilate = minDilate;
    }
}