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
    
    private readonly List<StatViewData> _statsBuffer = new(4);
    private Vector3 _originalScale;
    private Coroutine _scaleRoutine;

    private void Awake()
    {
        if (_portraitTransform == null)
        {
            Log.Error($"{nameof(CharacterSelectView)} missing portrait transform.", this);
            enabled = false;
            return;
        }

        _originalScale = _portraitTransform.localScale;
    }

    private void OnDisable()
    {
        if (_scaleRoutine != null)
        {
            StopCoroutine(_scaleRoutine);
            _scaleRoutine = null;
        }

        if (_portraitTransform)
            _portraitTransform.localScale = _originalScale;
    }

    public void DisplayCharacter(CharacterDefinition character)
    {
        if (!character)
            return;

        if (_portraitImage)
            _portraitImage.sprite = character.Portrait;

        if (_nameText)
            _nameText.text = character.DisplayName;

        if (_statsPanel)
        {
            _statsBuffer.Clear();
            _statsBuffer.Add(new StatViewData("HP", character.MaxHp));
            _statsBuffer.Add(new StatViewData("Attack", character.Attack));
            _statsBuffer.Add(new StatViewData("Armor", character.Armor));
            _statsBuffer.Add(new StatViewData("Speed", character.Speed));
            _statsPanel.Show(_statsBuffer);
        }

        PlaySelectionAnimation();
    }

    private void PlaySelectionAnimation()
    {
        if (!_portraitTransform)
            return;

        if (_scaleRoutine != null)
            StopCoroutine(_scaleRoutine);

        var from = _originalScale + Vector3.one * _scaleAmount;
        _scaleRoutine = UIAnimator.AnimateScale(_portraitTransform, from, _originalScale, _animationTime, this);
    }
}