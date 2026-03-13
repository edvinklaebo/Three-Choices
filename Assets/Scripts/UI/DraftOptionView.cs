using System;

using Core;

using UI.Tooltip;

using UnityEngine;
using UnityEngine.UI;

using Utils;

namespace UI
{
    [RequireComponent(typeof(Button))]
    public class DraftOptionView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Text _text;
        [SerializeField] private Image _icon;
        [SerializeField] private TooltipTrigger _tooltip;

        private DraftOption _boundOption;
        private Action<DraftOption> _onPick;
        private bool _initialized;

        public void Awake()
        {
            if (this._initialized) 
                return;
            this._initialized = true;

            if (this._button == null) this._button = GetComponent<Button>();
            if (this._text == null) this._text = GetComponentInChildren<Text>();
            if (this._icon == null) this._icon = GetComponentInChildren<Image>(true);
            if (this._tooltip == null) this._tooltip = GetComponent<TooltipTrigger>();

            this._button.onClick.AddListener(OnClicked);
        }

        public void Bind(DraftOption option, Action<DraftOption> onPick)
        {
            if (option == null)
            {
                Log.Error("Bind called with null option", new { gameObject.name });
                return;
            }

            this._boundOption = option;
            this._onPick = onPick;

            if (this._tooltip != null)
            {
                this._tooltip.Label = option.DisplayName;
                this._tooltip.Content = option.Description;
            }

            if (this._text != null)
                this._text.text = option.DisplayName;

            if (this._icon != null)
            {
                this._icon.sprite = option.Icon;
                this._icon.enabled = option.Icon != null;
            }
        }

        private void OnClicked()
        {
            this._onPick?.Invoke(this._boundOption);
        }
    }
}