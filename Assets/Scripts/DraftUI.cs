using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraftUI : MonoBehaviour
{
    public Button[] DraftButtons;

    private List<UpgradeDefinition> _currentDraft;
    private System.Action<UpgradeDefinition> _onPick;

    public void Show(List<UpgradeDefinition> draft, System.Action<UpgradeDefinition> onPick)
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

        Log.Info("Showing draft UI", new
        {
            draftCount = draft.Count,
            upgrades = draft.ConvertAll(u => u?.DisplayName),
            buttonCount = DraftButtons.Length
        });

        _currentDraft = draft;
        _onPick = onPick;

        for (var i = 0; i < DraftButtons.Length; i++)
        {
            var btn = DraftButtons[i];

            if (btn == null)
            {
                Log.Warn("Null button reference in DraftButtons array", new { index = i });
                continue;
            }

            if (i >= draft.Count)
            {
                Log.Warn("Not enough draft entries for buttons, disabling extra button", new
                {
                    buttonIndex = i,
                    draftCount = draft.Count
                });

                btn.gameObject.SetActive(false);
                continue;
            }

            btn.gameObject.SetActive(true);

            var text = btn.GetComponentInChildren<Text>();
            if (text == null)
            {
                Log.Warn("Draft button missing Text component", new { index = i });
            }
            else
            {
                text.text = draft[i].DisplayName;
            }

            var index = i; // closure safety
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => Pick(index));

            Log.Info("Draft button bound", new
            {
                index,
                upgrade = draft[i].DisplayName
            });
        }
    }

    private void Pick(int index)
    {
        if (_currentDraft == null)
        {
            Log.Error("Pick called but _currentDraft is null");
            return;
        }

        if (index < 0 || index >= _currentDraft.Count)
        {
            Log.Error("Pick called with out-of-range index", new
            {
                index,
                draftCount = _currentDraft.Count
            });
            return;
        }

        var picked = _currentDraft[index];

        Log.Info("Upgrade picked from draft", new
        {
            index,
            upgrade = picked?.DisplayName
        });

        if (_onPick == null)
        {
            Log.Warn("No _onPick callback set when picking upgrade");
        }
        else
        {
            _onPick.Invoke(picked);
        }

        foreach (var btn in DraftButtons)
        {
            if (btn == null) continue;
                btn.gameObject.SetActive(false);
        }

        Log.Info("Draft UI hidden after pick");
    }
}
