using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private string _effectId;
    private int _stacks;
    private int _duration;

    private void Awake()
    {
        if (_iconImage == null)
        {
            _iconImage = GetComponent<Image>();
        }
    }

    /// <summary>
    /// Set the effect data to display.
    /// </summary>
    public void SetEffect(string effectId, int stacks, int duration)
    {
        _effectId = effectId;
        _stacks = stacks;
        _duration = duration;

        // Set icon sprite (placeholder - would load from resources/data)
        if (_iconImage != null)
        {
            // TODO: Load sprite based on effectId from ScriptableObject or Resources
            // For now, just set a placeholder color
            _iconImage.color = GetEffectColor(effectId);
        }

        // Set stack count
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

        // Set duration
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

        // Update duration ring fill
        if (_durationRing != null)
        {
            _durationRing.gameObject.SetActive(duration > 0);
            // TODO: Animate fill amount based on max duration
        }
    }

    /// <summary>
    /// Set this icon to display overflow indicator.
    /// </summary>
    public void SetOverflow(int overflowCount)
    {
        if (_iconImage != null)
        {
            _iconImage.color = Color.gray;
        }

        if (_stackText != null)
        {
            _stackText.text = $"+{overflowCount}";
            _stackText.gameObject.SetActive(true);
        }

        if (_durationText != null)
        {
            _durationText.gameObject.SetActive(false);
        }

        if (_durationRing != null)
        {
            _durationRing.gameObject.SetActive(false);
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
