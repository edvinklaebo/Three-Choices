using Core;
using Core.Artifacts;

using Events;

using Systems;

using UnityEngine;

namespace Controllers
{
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
            if (this.upgradePicked != null) this.upgradePicked.OnRaised += ApplyUpgrade;
            if (this.artifactPicked != null) this.artifactPicked.OnRaised += ApplyArtifact;
            if (this._fightStarted != null) 
                this._fightStarted.OnRaised += OnFightStarted;
        }

        private void OnDisable()
        {
            if (this.upgradePicked != null) this.upgradePicked.OnRaised -= ApplyUpgrade;
            if (this.artifactPicked != null) this.artifactPicked.OnRaised -= ApplyArtifact;
            if (this._fightStarted != null) 
                this._fightStarted.OnRaised -= OnFightStarted;
        }

        private void OnFightStarted(Unit player, int fightIndex)
        {
            this._player = player;
        }

        private void ApplyUpgrade(UpgradeDefinition upgrade)
        {
            UpgradeApplier.Apply(upgrade, this._player);
            CompletePick();
        }

        private void ApplyArtifact(ArtifactDefinition artifact)
        {
            ArtifactApplier.ApplyToPlayer(artifact, this._player);
            CompletePick();
        }

        private void CompletePick()
        {
            this._hideDraft?.Raise();
            this.requestNextFight.Raise();
        }
    }
}