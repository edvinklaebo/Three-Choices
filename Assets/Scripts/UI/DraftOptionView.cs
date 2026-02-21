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
    private Action _cachedClick;

    public void Awake()
    {
        if (_button == null) _button = GetComponent<Button>();
        if (_text == null) _text = GetComponentInChildren<Text>();
        if (_icon == null) _icon = GetComponentInChildren<Image>(true);
        if (_tooltip == null) _tooltip = GetComponent<TooltipTrigger>();
    }

    public void Bind(UpgradeDefinition upgrade, Action<UpgradeDefinition> onPick)
    {
        if (upgrade == null)
        {
            Log.Error("DraftOptionView.Bind called with null upgrade", new { name = gameObject.name });
            return;
        }

        if (_tooltip != null)
        {
            _tooltip.Label = upgrade.DisplayName;
            _tooltip.Content = upgrade.Description;
        }

        if (_text != null)
            _text.text = upgrade.DisplayName;
        else
            Log.Warning("DraftOptionView missing Text component", new { name = gameObject.name });

        if (_icon != null)
        {
            if (upgrade.Icon != null)
            {
                _icon.sprite = upgrade.Icon;
                _icon.enabled = true;
            }
            else
            {
                _icon.enabled = false;
            }
        }

        if (_cachedClick != null) _button.onClick.RemoveListener(_cachedClick);
        _cachedClick = () => onPick?.Invoke(upgrade);
        _button.onClick.AddListener(_cachedClick);
    }
}
