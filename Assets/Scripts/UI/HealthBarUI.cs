using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private float _lerpSpeed = 5f;

    private Unit _unit;
    private float _targetFillAmount;
    private float _currentFillAmount;

    public void Initialize(Unit unit)
    {
        if (unit == null)
        {
            Debug.LogError("HealthBarUI: Cannot initialize with null unit");
            return;
        }

        _unit = unit;
        _targetFillAmount = GetNormalizedHealth();
        _currentFillAmount = _targetFillAmount;
        
        if (_fillImage != null)
        {
            _fillImage.fillAmount = _currentFillAmount;
        }
    }

    private void OnEnable()
    {
        if (_unit != null)
        {
            _unit.HealthChanged += OnHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (_unit != null)
        {
            _unit.HealthChanged -= OnHealthChanged;
        }
    }

    private void Update()
    {
        if (_fillImage == null)
            return;

        if (Mathf.Abs(_currentFillAmount - _targetFillAmount) > 0.001f)
        {
            _currentFillAmount = Mathf.Lerp(_currentFillAmount, _targetFillAmount, Time.deltaTime * _lerpSpeed);
            _fillImage.fillAmount = _currentFillAmount;
        }
    }

    private void OnHealthChanged(Unit unit, int currentHP, int maxHP)
    {
        _targetFillAmount = GetNormalizedHealth();
    }

    private float GetNormalizedHealth()
    {
        if (_unit == null || _unit.Stats.MaxHP <= 0)
            return 0f;

        return Mathf.Clamp01((float)_unit.Stats.CurrentHP / _unit.Stats.MaxHP);
    }
}
