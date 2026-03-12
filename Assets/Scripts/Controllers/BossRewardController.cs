using Core.Artifacts;
using Events;
using Systems;
using UnityEngine;
using Utils;

public class BossRewardController : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private VoidEventChannel _bossFightEnded;
    [SerializeField] private DraftEventChannel _showDraft;

    [Header("Config")]
    [SerializeField] private ArtifactPool _artifactPool;
    [SerializeField] private int _draftSize = 3;

    private DraftSystem _draftSystem;

    private void Awake()
    {
        Debug.Assert(_bossFightEnded != null);
        Debug.Assert(_showDraft != null);
        Debug.Assert(_artifactPool != null);

        _draftSystem = new DraftSystem(_artifactPool);
    }

    private void OnEnable()
    {
        _bossFightEnded.OnRaised += OnBossFightEnded;
    }

    private void OnDisable()
    {
        _bossFightEnded.OnRaised -= OnBossFightEnded;
    }

    private void OnBossFightEnded()
    {
        var draft = _draftSystem.GenerateDraft(_draftSize);

        if (draft.Count == 0)
        {
            Log.Warning("No artifact options available for boss reward draft.");
            return;
        }

        _showDraft.Raise(draft);
    }
}