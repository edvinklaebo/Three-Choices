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
    [SerializeField] private ArtifactRewardEventChannel _artifactPicked;

    private UIFader _fader;
    private DraftOptionView[] _draftOptions;

    public void Awake()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        _fader = new UIFader(canvasGroup, this);

        _draftOptions = BuildOptions();

        _fader.Hide(animated: false);
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

    private void OnShowRequested(List<DraftOption> draft)
    {
        if (_upgradePicked == null && _artifactPicked == null)
            Log.Warning("DraftUI: neither _upgradePicked nor _artifactPicked event channel is assigned. Picks will not be broadcast.");

        Show(draft, OnOptionPicked);
    }

    private void OnOptionPicked(DraftOption option)
    {
        switch (option.Source)
        {
            case UpgradeDefinition upgrade:
                _upgradePicked?.Raise(upgrade);
                break;
            case ArtifactDefinition artifact:
                _artifactPicked?.Raise(artifact);
                break;
            default:
                Log.Warning("DraftUI: unhandled IDraftable type picked",
                    new { type = option.Source.GetType().Name });
                break;
        }
    }

    private void OnHideRequested()
    {
        Hide(animated: false);
    }

    public void Show(List<DraftOption> draft, Action<DraftOption> onPick, bool animated = true)
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

            var draftOption = draft[i];
            option.Bind(draftOption, onPick);
        }
    }

    public void Hide(bool animated = true)
    {
        Log.Info("DraftUI.Hide invoked", new { animated });
        _fader.Hide(animated);
    }

    private DraftOptionView[] BuildOptions()
    {
        if (DraftButtons == null) return Array.Empty<DraftOptionView>();

        var options = new DraftOptionView[DraftButtons.Length];
        for (var i = 0; i < DraftButtons.Length; i++)
        {
            if (DraftButtons[i] == null) continue;
            var view = DraftButtons[i].GetComponent<DraftOptionView>();
            if (view == null)
            {
                view = DraftButtons[i].gameObject.AddComponent<DraftOptionView>();
                view.Awake();
            }
            options[i] = view;
        }
        return options;
    }
}