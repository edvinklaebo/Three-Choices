using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraftUI : MonoBehaviour
{
    public Button[] DraftButtons;

    [SerializeField] private VoidEventChannel _onHideRequested;
    [SerializeField] private DraftEventChannel _onShowRequested;
    [SerializeField] private UpgradeEventChannel _upgradePicked;

    private UIFader _fader;
    private DraftOptionView[] _draftOptions;
    private List<UpgradeDefinition> _currentDraft;

    public bool DidAwake { get; private set; }

    public void Awake()
    {
        var canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        _fader = new UIFader(canvasGroup, this);

        _draftOptions = BuildOptions();

        _fader.Hide(animated: false);

        DidAwake = true;
    }

    private void OnEnable()
    {
        if (_onHideRequested != null) _onHideRequested.OnRaised += OnHideRequested;
        if (_onShowRequested != null) _onShowRequested.OnRaised += OnShowRequested;
    }

    private void OnDisable()
    {
        if (_onHideRequested != null) _onHideRequested.OnRaised -= OnHideRequested;
        if (_onShowRequested != null) _onShowRequested.OnRaised -= OnShowRequested;
    }

    private void OnShowRequested(List<UpgradeDefinition> draft)
    {
        if (_upgradePicked == null)
            Log.Warning("DraftUI: _upgradePicked event channel is not assigned. Upgrade picks will not be broadcast.");

        Show(draft, u => _upgradePicked?.Raise(u));
    }

    private void OnHideRequested()
    {
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

        // Rebuild option views if DraftButtons were assigned after Awake
        if (_draftOptions == null || _draftOptions.Length != DraftButtons.Length)
            _draftOptions = BuildOptions();

        _currentDraft = draft;

        gameObject.SetActive(true);
        _fader.Show(animated);

        for (var i = 0; i < _draftOptions.Length; i++)
        {
            var option = _draftOptions[i];

            if (option == null)
            {
                Log.Warning("Null option in DraftButtons array", new { index = i });
                continue;
            }

            if (i >= draft.Count)
            {
                option.gameObject.SetActive(false);
                continue;
            }

            option.gameObject.SetActive(true);

            var upgrade = draft[i];
            var captured = i;
            option.Bind(upgrade, () => Pick(captured, onPick));
        }
    }

    public void Hide(bool animated = true)
    {
        Log.Info("DraftUI.Hide invoked", new { animated });
        _fader.Hide(animated);
    }

    private void Pick(int index, Action<UpgradeDefinition> onPick)
    {
        if (_currentDraft == null || index < 0 || index >= _currentDraft.Count)
            return;

        onPick?.Invoke(_currentDraft[index]);
    }

    private DraftOptionView[] BuildOptions()
    {
        if (DraftButtons == null) return Array.Empty<DraftOptionView>();

        var options = new DraftOptionView[DraftButtons.Length];
        for (var i = 0; i < DraftButtons.Length; i++)
        {
            if (DraftButtons[i] == null) continue;
            var view = DraftButtons[i].GetComponent<DraftOptionView>()
                       ?? DraftButtons[i].gameObject.AddComponent<DraftOptionView>();
            if (!view.DidAwake) view.Awake();
            options[i] = view;
        }
        return options;
    }
}