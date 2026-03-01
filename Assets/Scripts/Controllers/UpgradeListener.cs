using UnityEngine;

public class UpgradeListener : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UpgradeEventChannel upgradePicked;
    [SerializeField] private ArtifactRewardEventChannel artifactPicked;
    [SerializeField] private VoidEventChannel requestNextFight;
    [SerializeField] private FightStartedEventChannel _fightStarted;
    [SerializeField] private VoidEventChannel _hideDraft;

    private Unit _player;

    private void OnEnable()
    {
        if (upgradePicked != null) upgradePicked.OnRaised += ApplyUpgrade;
        if (artifactPicked != null) artifactPicked.OnRaised += ApplyArtifact;
        if (_fightStarted != null) 
            _fightStarted.OnRaised += OnFightStarted;
    }

    private void OnDisable()
    {
        if (upgradePicked != null) upgradePicked.OnRaised -= ApplyUpgrade;
        if (artifactPicked != null) artifactPicked.OnRaised -= ApplyArtifact;
        if (_fightStarted != null) 
            _fightStarted.OnRaised -= OnFightStarted;
    }

    private void OnFightStarted(Unit player, int fightIndex)
    {
        _player = player;
    }

    private void ApplyUpgrade(UpgradeDefinition upgrade)
    {
        UpgradeApplier.Apply(upgrade, _player);
        CompletePick();
    }

    private void ApplyArtifact(ArtifactDefinition artifact)
    {
        ArtifactApplier.ApplyToPlayer(artifact, _player);
        CompletePick();
    }

    private void CompletePick()
    {
        _hideDraft?.Raise();
        requestNextFight.Raise();
    }
}