using TMPro;
using UnityEngine;

/// <summary>
///     Turn indicator UI component.
///     Displays which unit's turn is active.
///     Driven by combat events.
/// </summary>
public class TurnIndicatorUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if (_turnText == null) _turnText = GetComponentInChildren<TextMeshProUGUI>();

        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

        Hide();
    }

    /// <summary>
    ///     Show turn indicator for active unit.
    /// </summary>
    public void ShowTurn(Unit activeUnit)
    {
        if (activeUnit == null)
            return;

        if (_turnText != null) _turnText.text = $"{activeUnit.Name}'s Turn";

        if (_canvasGroup != null) _canvasGroup.alpha = 1f;
    }

    /// <summary>
    ///     Hide turn indicator.
    /// </summary>
    public void Hide()
    {
        if (_canvasGroup != null) _canvasGroup.alpha = 0f;
    }
}