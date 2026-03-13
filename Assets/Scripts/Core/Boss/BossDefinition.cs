using Core.Artifacts;

using UnityEngine;

namespace Core.Boss
{
    /// <summary>
    ///     ScriptableObject that describes a boss: its phases, reward artifact, and difficulty rating.
    ///     Create via the asset menu: Game/Boss/Boss Definition.
    ///
    ///     Phase ordering: phases must be sorted from highest to lowest <c>TriggerHPPercent</c>.
    ///     The first phase must have a trigger of 100.
    ///
    ///     Difficulty rating: compared against the current tier (<c>fightIndex / 10</c>) by <see cref="BossManager"/>.
    ///     A rating of 1 makes this boss available from fight 10 onward; rating 5 from fight 50 onward.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Boss/Boss Definition")]
    public class BossDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;

        [Header("Prefab")]
        [SerializeField] private GameObject _prefab;

        [Header("Phases")]
        [SerializeField] private BossPhaseDefinition[] _phases;

        [Header("Reward")]
        [SerializeField] private ArtifactDefinition _artifactReward;

        [Header("Stats")]
        [SerializeField] private Stats _stats = new Stats();

        [Header("Difficulty")]
        [SerializeField] private int _difficultyRating = 1;

        public string Id => this._id;
        public string DisplayName => this._displayName;
        public GameObject Prefab => this._prefab;
        public BossPhaseDefinition[] Phases => this._phases;
        public ArtifactDefinition ArtifactReward => this._artifactReward;
        public Stats Stats => this._stats;
        public int DifficultyRating => this._difficultyRating;

#if UNITY_EDITOR
        public void EditorInit(string id, string displayName, int difficultyRating,
                               ArtifactDefinition artifactReward = null, BossPhaseDefinition[] phases = null,
                               Stats stats = null)
        {
            this._id = id;
            this._displayName = displayName;
            this._difficultyRating = difficultyRating;
            this._artifactReward = artifactReward;
            this._phases = phases ?? System.Array.Empty<BossPhaseDefinition>();
            this._stats = stats ?? new Stats();
        }
#endif
    }
}
