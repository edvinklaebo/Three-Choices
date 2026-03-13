using TMPro;

using UnityEngine;

namespace Utils
{
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
            this.text = GetComponent<TMP_Text>();
            this.material = this.text.fontMaterial;
        }

        private void Update()
        {
            var t = (Mathf.Sin(Time.time * this.speed) + 1f) * 0.5f;
            var dilate = Mathf.Lerp(this.minDilate, this.maxDilate, t);

            this.material.SetFloat(FaceDilate, dilate);
        }

        private void OnValidate()
        {
            if (this.maxDilate < this.minDilate)
                this.maxDilate = this.minDilate;
        }
    }
}