using System;
using System.Collections.Generic;

using Core;
using Core.Artifacts;

using Events;

using UnityEngine;
using UnityEngine.UI;

using Utils;

namespace UI
{
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
            this._fader = new UIFader(canvasGroup, this);

            this._draftOptions = BuildOptions();

            this._fader.Hide(animated: false);
        }

        private void OnEnable()
        {
            if (this._onHideRequested != null) this._onHideRequested.OnRaised += OnHideRequested;
            if (this._onShowRequested != null) this._onShowRequested.OnRaised += OnShowRequested;
        }

        private void OnDisable()
        {
            if (this._onHideRequested != null) this._onHideRequested.OnRaised -= OnHideRequested;
            if (this._onShowRequested != null) this._onShowRequested.OnRaised -= OnShowRequested;
        }

        private void OnShowRequested(List<DraftOption> draft)
        {
            if (this._upgradePicked == null && this._artifactPicked == null)
                Log.Warning("DraftUI: neither _upgradePicked nor _artifactPicked event channel is assigned. Picks will not be broadcast.");

            Show(draft, OnOptionPicked);
        }

        private void OnOptionPicked(DraftOption option)
        {
            switch (option.Source)
            {
                case UpgradeDefinition upgrade:
                    this._upgradePicked?.Raise(upgrade);
                    break;
                case ArtifactDefinition artifact:
                    this._artifactPicked?.Raise(artifact);
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

            if (this.DraftButtons == null || this.DraftButtons.Length == 0)
            {
                Log.Error("DraftUI has no buttons assigned");
                return;
            }

            // Rebuild option views if DraftButtons were assigned after Awake
            if (this._draftOptions == null || this._draftOptions.Length != this.DraftButtons.Length)
                this._draftOptions = BuildOptions();

            gameObject.SetActive(true);
            this._fader.Show(animated);

            for (var i = 0; i < this._draftOptions.Length; i++)
            {
                var option = this._draftOptions[i];

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
            this._fader.Hide(animated);
        }

        private DraftOptionView[] BuildOptions()
        {
            if (this.DraftButtons == null) return Array.Empty<DraftOptionView>();

            var options = new DraftOptionView[this.DraftButtons.Length];
            for (var i = 0; i < this.DraftButtons.Length; i++)
            {
                if (this.DraftButtons[i] == null) continue;
                var view = this.DraftButtons[i].GetComponent<DraftOptionView>();
                if (view == null)
                {
                    view = this.DraftButtons[i].gameObject.AddComponent<DraftOptionView>();
                    view.Awake();
                }
                options[i] = view;
            }
            return options;
        }
    }
}