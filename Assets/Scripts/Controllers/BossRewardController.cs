using Core.Artifacts;

using Events;

using Systems;

using UnityEngine;

using Utils;

namespace Controllers
{
    /// <summary>
    ///     Listens for boss fight end and presents a rarity-weighted artifact draft
    ///     via a <see cref="DraftEventChannel"/> so the player can choose their boss reward.
    /// </summary>
    public class BossRewardController : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private VoidEventChannel _bossFightEnded;
        [SerializeField] private DraftEventChannel _showDraft;

        private DraftSystem _draftSystem;

        private void Awake()
        {
            if (this._bossFightEnded == null)
                Log.Error("BossRewardController: _bossFightEnded is not assigned.");
            if (this._showDraft == null)
                Log.Error("BossRewardController: _showDraft is not assigned.");

            this._draftSystem = new DraftSystem(ScriptableObject.CreateInstance<ArtifactPool>());
        }

        private void OnEnable()
        {
            if (this._bossFightEnded != null)
                this._bossFightEnded.OnRaised += OnBossFightEnded;
        }

        private void OnDisable()
        {
            if (this._bossFightEnded != null)
                this._bossFightEnded.OnRaised -= OnBossFightEnded;
        }

        private void OnBossFightEnded()
        {
            var draft = this._draftSystem.GenerateDraft(3);

            if (draft.Count == 0)
            {
                Log.Warning("[BossRewardController] No artifact options available for boss reward draft.");
                return;
            }

            Log.Info($"[BossRewardController] Presenting artifact draft with {draft.Count} option(s).");
            this._showDraft?.Raise(draft);
        }
    }
}
