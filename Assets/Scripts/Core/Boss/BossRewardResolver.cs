/// <summary>
///     Resolves the artifact reward for a defeated boss.
///     Returns the artifact assigned to the boss definition.
///     Future extensions: artifact pools, rarity weighting, meta-progression locks.
/// </summary>
public class BossRewardResolver
{
    /// <summary>Returns the <see cref="ArtifactDefinition"/> assigned to the given boss.</summary>
    public ArtifactDefinition ResolveReward(BossDefinition boss)
    {
        if (boss == null)
        {
            Log.Error("[BossRewardResolver] Cannot resolve reward for null boss");
            return null;
        }

        return boss.ArtifactReward;
    }
}
