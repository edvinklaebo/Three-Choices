using Core.StatusEffects;

using TMPro;

using UI.Tooltip;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Combat
{
    /// <summary>
    ///     Individual status effect icon display.
    ///     Shows effect sprite, stack count, and optional duration.
    ///     Supports hoverable tooltip using the StatusEffectDefinition metadata.
    /// </summary>
    public class StatusEffectIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _stackText;
        [SerializeField] private TextMeshProUGUI _durationText;
        [SerializeField] private Image _durationRing;

        private StatusEffectDefinition _definition;

        private void Awake()
        {
            if (_iconImage == null)
                _iconImage = GetComponent<Image>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_definition == null)
                return;

            TooltipSystem.Show(_definition.Description, _definition.DisplayName);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipSystem.Hide();
        }

        /// <summary>
        ///     Set the effect data to display.
        ///     Pass an optional <paramref name="definition" /> to use its icon and enable the hover tooltip.
        /// </summary>
        public void SetEffect(string effectId, int stacks, int duration, StatusEffectDefinition definition = null)
        {
            _definition = definition;

            if (_iconImage != null)
            {
                if (definition != null && definition.Icon != null)
                {
                    _iconImage.sprite = definition.Icon;
                    _iconImage.color = definition.Color;
                }
                else
                {
                    _iconImage.sprite = null;
                    _iconImage.color = GetFallbackColor(effectId);
                }
            }

            if (_stackText != null)
            {
                if (stacks > 1)
                {
                    _stackText.text = stacks.ToString();
                    _stackText.gameObject.SetActive(true);
                }
                else
                {
                    _stackText.gameObject.SetActive(false);
                }
            }

            if (_durationText != null)
            {
                if (duration > 0)
                {
                    _durationText.text = duration.ToString();
                    _durationText.gameObject.SetActive(true);
                }
                else
                {
                    _durationText.gameObject.SetActive(false);
                }
            }

            if (_durationRing != null)
                _durationRing.gameObject.SetActive(duration > 0);
        }

        /// <summary>
        ///     Set this icon to display an overflow indicator ("+N more effects").
        ///     Overflow icons do not show tooltips.
        /// </summary>
        public void SetOverflow(int overflowCount)
        {
            _definition = null;

            if (_iconImage != null)
                _iconImage.color = Color.gray;

            if (_stackText != null)
            {
                _stackText.text = $"+{overflowCount}";
                _stackText.gameObject.SetActive(true);
            }

            if (_durationText != null)
                _durationText.gameObject.SetActive(false);

            if (_durationRing != null)
                _durationRing.gameObject.SetActive(false);
        }

        /// <summary>
        ///     Fallback color when no <see cref="StatusEffectDefinition" /> is available.
        /// </summary>
        private static Color GetFallbackColor(string effectId)
        {
            return effectId.ToLower() switch
            {
                "poison" => new Color(0.5f, 0f, 0.8f),
                "bleed"  => new Color(0.8f, 0f, 0f),
                "burn"   => new Color(1f, 0.5f, 0f),
                "stun"   => new Color(1f, 1f, 0f),
                _        => Color.white
            };
        }
    }
}
