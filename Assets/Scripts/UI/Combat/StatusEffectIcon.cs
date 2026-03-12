using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace UI.Combat
{
    /// <summary>
    /// Individual status effect icon display.
    /// Shows effect sprite, stack count, and optional duration.
    /// </summary>
    public class StatusEffectIcon : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _stackText;
        [SerializeField] private TextMeshProUGUI _durationText;
        [SerializeField] private Image _durationRing;


        private void Awake()
        {
            if (this._iconImage == null)
            {
                this._iconImage = GetComponent<Image>();
            }
        }

        /// <summary>
        /// Set the effect data to display.
        /// </summary>
        public void SetEffect(string effectId, int stacks, int duration)
        {
            // Set icon sprite (placeholder - would load from resources/data)
            if (this._iconImage != null)
            {
                // For now, just set a placeholder color
                this._iconImage.color = GetEffectColor(effectId);
            }

            // Set stack count
            if (this._stackText != null)
            {
                if (stacks > 1)
                {
                    this._stackText.text = stacks.ToString();
                    this._stackText.gameObject.SetActive(true);
                }
                else
                {
                    this._stackText.gameObject.SetActive(false);
                }
            }

            // Set duration
            if (this._durationText != null)
            {
                if (duration > 0)
                {
                    this._durationText.text = duration.ToString();
                    this._durationText.gameObject.SetActive(true);
                }
                else
                {
                    this._durationText.gameObject.SetActive(false);
                }
            }

            // Update duration ring fill
            if (this._durationRing != null)
            {
                this._durationRing.gameObject.SetActive(duration > 0);
            }
        }

        /// <summary>
        /// Set this icon to display overflow indicator.
        /// </summary>
        public void SetOverflow(int overflowCount)
        {
            if (this._iconImage != null)
            {
                this._iconImage.color = Color.gray;
            }

            if (this._stackText != null)
            {
                this._stackText.text = $"+{overflowCount}";
                this._stackText.gameObject.SetActive(true);
            }

            if (this._durationText != null)
            {
                this._durationText.gameObject.SetActive(false);
            }

            if (this._durationRing != null)
            {
                this._durationRing.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Get placeholder color for effect type.
        /// In production, this would load actual sprites.
        /// </summary>
        private Color GetEffectColor(string effectId)
        {
            return effectId.ToLower() switch
            {
                "poison" => new Color(0.5f, 0f, 0.8f),  // Purple
                "bleed" => new Color(0.8f, 0f, 0f),     // Red
                "burn" => new Color(1f, 0.5f, 0f),      // Orange
                "stun" => new Color(1f, 1f, 0f),        // Yellow
                _ => Color.white
                };
        }
    }
}
