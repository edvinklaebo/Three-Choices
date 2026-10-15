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
        [SerializeField] private Image _durationRing;

        private StatusEffectDefinition _definition;
        private string _effectId;
        private string _tooltipContent;
        private int _stacks;
        private int _duration;
        private int _overflowCount = -1;
        private bool _isHovered;

        private void Awake()
        {
            if (_iconImage == null)
                _iconImage = GetComponent<Image>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_definition == null)
                return;

            _isHovered = true;
            TooltipSystem.Show(_tooltipContent, _definition.DisplayName, this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        private void OnDisable()
        {
            HideTooltip();
        }

        private void HideTooltip()
        {
            _isHovered = false;
            TooltipSystem.Hide(this);
        }

        internal bool MatchesEffect(string effectId, int stacks, int duration)
        {
            return _effectId == effectId && _stacks == stacks && _duration == duration;
        }

        /// <summary>
        ///     Set the effect data to display.
        ///     Pass an optional <paramref name="definition" /> to use its icon and enable the hover tooltip.
        /// </summary>
        public void SetEffect(string effectId, int stacks, int duration, StatusEffectDefinition definition = null)
        {
            _definition = definition;
            _effectId = effectId;
            _stacks = stacks;
            _duration = duration;
            _overflowCount = -1;
            UpdateTooltip();

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

            if (_durationRing != null)
                _durationRing.gameObject.SetActive(duration > 0);
        }

        private void UpdateTooltip()
        {
            if (_definition == null)
            {
                _tooltipContent = string.Empty;
                HideTooltip();
                return;
            }

            _tooltipContent = $"{_definition.Description}\n\nStacks: {_stacks}";
            if (_duration > 0)
                _tooltipContent += $"\nTurns remaining: {_duration}";

            if (_isHovered)
                TooltipSystem.Show(_tooltipContent, _definition.DisplayName, this);
        }

        /// <summary>
        ///     Set this icon to display an overflow indicator ("+N more effects").
        ///     Overflow icons do not show tooltips.
        /// </summary>
        public void SetOverflow(int overflowCount)
        {
            if (_overflowCount == overflowCount)
                return;

            HideTooltip();
            _overflowCount = overflowCount;
            _definition = null;
            _effectId = null;

            if (_iconImage != null)
            {
                _iconImage.sprite = null;
                _iconImage.color = Color.gray;
            }

            if (_stackText != null)
            {
                _stackText.text = $"+{overflowCount}";
                _stackText.gameObject.SetActive(true);
            }

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
