using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraftUI : MonoBehaviour
{
    public static DraftUI Instance;
    public Button[] DraftButtons;

    private List<UpgradeDefinition> _currentDraft;
    private Action<UpgradeDefinition> _onPick;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Log.Warning($"Duplicate {nameof(DraftUI)} detected. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Show(List<UpgradeDefinition> draft, Action<UpgradeDefinition> onPick)
    {
        Log.Info("DraftUI.Show invoked", new
        {
            thisInstanceId = GetInstanceID(),
            onPickIsNull = onPick == null
        });

        if (draft == null || draft.Count == 0)
        {
            Log.Error("DraftUI.Show called with empty or null draft", new { draftCount = draft?.Count });
            return;
        }

        if (DraftButtons == null || DraftButtons.Length == 0)
        {
            Log.Error("DraftUI has no buttons assigned");
            return;
        }

        _currentDraft = draft;
        _onPick = onPick;

        for (var i = 0; i < DraftButtons.Length; i++)
        {
            var btn = DraftButtons[i];

            if (btn == null)
            {
                Log.Warning("Null button reference in DraftButtons array", new { index = i });
                continue;
            }

            if (i >= draft.Count)
            {
                btn.gameObject.SetActive(false);
                continue;
            }

            btn.gameObject.SetActive(true);

            var upgrade = draft[i];

            // --- Tooltip ---
            var tooltip = btn.GetComponent<TooltipTrigger>();
            tooltip.Content = _currentDraft[i].Description;
            tooltip.Label = _currentDraft[i].DisplayName;

            // --- Text ---
            var text = btn.GetComponentInChildren<Text>();
            if (text != null)
                text.text = upgrade.DisplayName;
            else
                Log.Warning("Draft button missing Text component", new { index = i });

            // --- Icon (NEW) ---
            var icon = btn.GetComponentInChildren<Image>(true);
            if (icon != null && upgrade.Icon != null)
            {
                icon.sprite = upgrade.Icon;
                icon.enabled = true;
            }
            else if (icon != null)
            {
                icon.enabled = false; // avoids stale sprites
            }

            var index = i; // closure safety
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => Pick(index));
        }
    }

    private void Pick(int index)
    {
        if (_currentDraft == null || index < 0 || index >= _currentDraft.Count)
            return;

        var picked = _currentDraft[index];
        _onPick?.Invoke(picked);
    }
}