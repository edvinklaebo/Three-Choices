using UnityEngine;

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
        if (_bossFightEnded == null)
            Log.Error("BossRewardController: _bossFightEnded is not assigned.");
        if (_showDraft == null)
            Log.Error("BossRewardController: _showDraft is not assigned.");

        _draftSystem = new DraftSystem(ScriptableObject.CreateInstance<ArtifactPool>());
    }

    private void OnEnable()
    {
        if (_bossFightEnded != null)
            _bossFightEnded.OnRaised += OnBossFightEnded;
    }

    private void OnDisable()
    {
        if (_bossFightEnded != null)
            _bossFightEnded.OnRaised -= OnBossFightEnded;
    }

    private void OnBossFightEnded()
    {
        var draft = _draftSystem.GenerateDraft(3);

        if (draft.Count == 0)
        {
            Log.Warning("[BossRewardController] No artifact options available for boss reward draft.");
            return;
        }

        Log.Info($"[BossRewardController] Presenting artifact draft with {draft.Count} option(s).");
        _showDraft?.Raise(draft);
    }
}
