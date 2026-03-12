using Core;

using TMPro;

using UnityEngine;

namespace UI.Combat
{
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
            if (this._turnText == null) this._turnText = GetComponentInChildren<TextMeshProUGUI>();

            if (this._canvasGroup == null) this._canvasGroup = GetComponent<CanvasGroup>();

            Hide();
        }

        /// <summary>
        ///     Show turn indicator for active unit.
        /// </summary>
        public void ShowTurn(Unit activeUnit)
        {
            if (activeUnit == null)
                return;

            if (this._turnText != null) this._turnText.text = $"{activeUnit.Name}'s Turn";

            if (this._canvasGroup != null) this._canvasGroup.alpha = 1f;
        }

        /// <summary>
        ///     Hide turn indicator.
        /// </summary>
        public void Hide()
        {
            if (this._canvasGroup != null) this._canvasGroup.alpha = 0f;
        }
    }
}