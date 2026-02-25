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

    private UpgradeDefinition _boundUpgrade;
    private Action<UpgradeDefinition> _onPick;
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

    public void Bind(UpgradeDefinition upgrade, Action<UpgradeDefinition> onPick)
    {
        if (upgrade == null)
        {
            Log.Error("Bind called with null upgrade", new { name = gameObject.name });
            return;
        }

        _boundUpgrade = upgrade;
        _onPick = onPick;

        if (_tooltip != null)
        {
            _tooltip.Label = upgrade.DisplayName;
            _tooltip.Content = upgrade.Description;
        }

        if (_text != null)
            _text.text = upgrade.DisplayName;

        if (_icon != null)
        {
            _icon.sprite = upgrade.Icon;
            _icon.enabled = upgrade.Icon != null;
        }
    }

    private void OnClicked()
    {
        _onPick?.Invoke(_boundUpgrade);
    }
}