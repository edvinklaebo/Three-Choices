using UnityEngine;

/// <summary>
///     Listens for <see cref="BossFightEventChannel"/> to track the active boss,
///     then resolves and broadcasts the artifact reward via <see cref="ArtifactRewardEventChannel"/>
///     when the boss fight ends.
/// </summary>
public class BossRewardController : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private BossFightEventChannel _bossFightStarted;
    [SerializeField] private VoidEventChannel _bossFightEnded;
    [SerializeField] private ArtifactRewardEventChannel _artifactReward;

    private BossRewardResolver _resolver;
    private BossDefinition _activeBoss;

    private void Awake()
    {
        _resolver = new BossRewardResolver();

        if (_bossFightStarted == null)
            Log.Error("BossRewardController: _bossFightStarted is not assigned.");
        if (_bossFightEnded == null)
            Log.Error("BossRewardController: _bossFightEnded is not assigned.");
        if (_artifactReward == null)
            Log.Error("BossRewardController: _artifactReward is not assigned.");
    }

    private void OnEnable()
    {
        if (_bossFightStarted != null)
            _bossFightStarted.OnRaised += OnBossFightStarted;
        if (_bossFightEnded != null)
            _bossFightEnded.OnRaised += OnBossFightEnded;
    }

    private void OnDisable()
    {
        if (_bossFightStarted != null)
            _bossFightStarted.OnRaised -= OnBossFightStarted;
        if (_bossFightEnded != null)
            _bossFightEnded.OnRaised -= OnBossFightEnded;
    }

    private void OnBossFightStarted(BossDefinition boss)
    {
        _activeBoss = boss;
    }

    private void OnBossFightEnded()
    {
        if (_activeBoss == null)
        {
            Log.Warning("[BossRewardController] Boss fight ended but no active boss was tracked.");
            return;
        }

        var reward = _resolver.ResolveReward(_activeBoss);

        if (reward == null)
        {
            Log.Warning($"[BossRewardController] Boss '{_activeBoss.Id}' has no artifact reward assigned.");
        }
        else
        {
            Log.Info($"[BossRewardController] Rewarding artifact '{reward.Id}' from boss '{_activeBoss.Id}'");
            _artifactReward?.Raise(reward);
        }

        _activeBoss = null;
    }
}
