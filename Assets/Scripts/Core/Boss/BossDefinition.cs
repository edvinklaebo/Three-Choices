using UnityEngine;

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

    [Header("Difficulty")]
    [SerializeField] private int _difficultyRating = 1;

    public string Id => _id;
    public string DisplayName => _displayName;
    public GameObject Prefab => _prefab;
    public BossPhaseDefinition[] Phases => _phases;
    public ArtifactDefinition ArtifactReward => _artifactReward;
    public int DifficultyRating => _difficultyRating;

#if UNITY_EDITOR
    public void EditorInit(string id, string displayName, int difficultyRating,
        ArtifactDefinition artifactReward = null, BossPhaseDefinition[] phases = null)
    {
        _id = id;
        _displayName = displayName;
        _difficultyRating = difficultyRating;
        _artifactReward = artifactReward;
        _phases = phases ?? System.Array.Empty<BossPhaseDefinition>();
    }
#endif
}
