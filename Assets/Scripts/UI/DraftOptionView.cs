using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DraftOptionView : MonoBehaviour
{
    private Button _button;
    private Text _text;
    private Image _icon;
    private TooltipTrigger _tooltip;
    private Action _cachedClick;

    public bool DidAwake { get; private set; }

    public void Awake()
    {
        _button = GetComponent<Button>();
        _text = GetComponentInChildren<Text>();
        _icon = GetComponentInChildren<Image>(true);
        _tooltip = GetComponent<TooltipTrigger>();
        DidAwake = true;
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
