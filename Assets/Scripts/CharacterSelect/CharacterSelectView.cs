using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _portraitImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private StatsPanelUI _statsPanel;
    [SerializeField] private Transform _portraitTransform;
    
    [Header("Configuration")]
    [SerializeField] private float _animationTime = 0.2f;
    [SerializeField] private float _scaleAmount = 0.1f;
    
    private Coroutine _scaleRoutine;
    private Vector3 _originalScale = Vector3.one;

    private void Awake()
    {
        if (_portraitTransform != null) 
            _originalScale = _portraitTransform.localScale;
    }

    public void DisplayCharacter(CharacterDefinition character)
    {
        if (!character)
            return;

        if (_portraitImage && character.Portrait) 
            _portraitImage.sprite = character.Portrait;

        if (_nameText) 
            _nameText.text = character.DisplayName;

        if (_statsPanel)
        {
            var stats = new List<StatViewData>
            {
                new("HP", character.MaxHp),
                new("Attack", character.Attack),
                new("Armor", character.Armor),
                new("Speed", character.Speed)
            };
            _statsPanel.Show(stats);
        }

        PlaySelectionAnimation();
    }

    private void PlaySelectionAnimation()
    {
        if (!_portraitTransform)
            return;

        if (_scaleRoutine != null)
            StopCoroutine(_scaleRoutine);

        _portraitTransform.localScale = _originalScale + Vector3.one * _scaleAmount;
        _scaleRoutine = StartCoroutine(AnimateScale());
    }

    private IEnumerator AnimateScale()
    {
        var elapsed = 0f;
        var startScale = _portraitTransform.localScale;

        while (elapsed < _animationTime)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / _animationTime);
            _portraitTransform.localScale = Vector3.Lerp(startScale, _originalScale, t);
            yield return null;
        }

        _portraitTransform.localScale = _originalScale;
        _scaleRoutine = null;
    }
}