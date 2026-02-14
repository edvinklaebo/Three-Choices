using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CharacterSelectView : MonoBehaviour
{
    [SerializeField] private Image _portraitImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private StatsPanelUI _statsPanel;
    [SerializeField] private Transform _portraitTransform;
    [SerializeField] private float _animationTime = 0.2f;
    [SerializeField] private float _scaleAmount = 0.1f;

    private Vector3 _originalScale = Vector3.one;

    private void Awake()
    {
        if (_portraitTransform != null)
        {
            _originalScale = _portraitTransform.localScale;
        }
    }

    public void DisplayCharacter(CharacterDefinition character)
    {
        if (character == null)
            return;

        if (_portraitImage != null && character.Portrait != null)
        {
            _portraitImage.sprite = character.Portrait;
        }

        if (_nameText != null)
        {
            _nameText.text = character.DisplayName;
        }

        if (_statsPanel != null)
        {
            var stats = new List<StatViewData>
            {
                new StatViewData("HP", character.MaxHp),
                new StatViewData("Attack", character.Attack),
                new StatViewData("Armor", character.Armor),
                new StatViewData("Speed", character.Speed)
            };
            _statsPanel.Show(stats);
        }

        PlaySelectionAnimation();
    }

    private void PlaySelectionAnimation()
    {
        if (_portraitTransform != null)
        {
            // Simple scale punch animation
            _portraitTransform.localScale = _originalScale + Vector3.one * _scaleAmount;
            StartCoroutine(AnimateScale());
        }
    }

    private System.Collections.IEnumerator AnimateScale()
    {
        float elapsed = 0f;
        Vector3 startScale = _portraitTransform.localScale;

        while (elapsed < _animationTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _animationTime;
            _portraitTransform.localScale = Vector3.Lerp(startScale, _originalScale, t);
            yield return null;
        }

        _portraitTransform.localScale = _originalScale;
    }
}
