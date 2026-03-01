using System;
using UnityEngine;
using UnityEngine.UI;

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
        if (_initialized) 
            return;
        _initialized = true;

        if (_button == null) _button = GetComponent<Button>();
        if (_text == null) _text = GetComponentInChildren<Text>();
        if (_icon == null) _icon = GetComponentInChildren<Image>(true);
        if (_tooltip == null) _tooltip = GetComponent<TooltipTrigger>();

        _button.onClick.AddListener(OnClicked);
    }

    public void Bind(DraftOption option, Action<DraftOption> onPick)
    {
        if (option == null)
        {
            Log.Error("Bind called with null option", new { gameObject.name });
            return;
        }

        _boundOption = option;
        _onPick = onPick;

        if (_tooltip != null)
        {
            _tooltip.Label = option.DisplayName;
            _tooltip.Content = option.Description;
        }

        if (_text != null)
            _text.text = option.DisplayName;

        if (_icon != null)
        {
            _icon.sprite = option.Icon;
            _icon.enabled = option.Icon != null;
        }
    }

    private void OnClicked()
    {
        _onPick?.Invoke(_boundOption);
    }
}