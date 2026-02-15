using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraftUI : MonoBehaviour
{
    public static DraftUI Instance;
    public Button[] DraftButtons;

    private List<UpgradeDefinition> _currentDraft;
    private Action<UpgradeDefinition> _onPick;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Log.Warning($"Duplicate {nameof(DraftUI)} detected. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Get or add CanvasGroup for fade animation
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Start hidden (no animation in Awake)
        Hide(animated: false);
    }

    public void Show(List<UpgradeDefinition> draft, Action<UpgradeDefinition> onPick, bool animated = true)
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

        // Make UI visible
        gameObject.SetActive(true);
        
        if (animated)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

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

    public void Hide(bool animated = true)
    {
        Log.Info("DraftUI.Hide invoked", new { animated });

        if (animated)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    private IEnumerator FadeIn()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    private IEnumerator FadeOut()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
    }
}