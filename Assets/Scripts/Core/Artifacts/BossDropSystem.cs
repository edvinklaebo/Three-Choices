using System;
using System.Collections.Generic;

public class BossDropSystem
{
    private const int DropCount = 3;

    private readonly IArtifactRepository _repository;
    private readonly IArtifactSelector _selector;
    private readonly ArtifactMetaProgression _metaProgression;

    public BossDropSystem(IArtifactRepository repository, ArtifactMetaProgression metaProgression)
        : this(repository, metaProgression, new RandomArtifactSelector())
    {
    }

    public BossDropSystem(IArtifactRepository repository, ArtifactMetaProgression metaProgression,
        IArtifactSelector selector)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _metaProgression = metaProgression ?? throw new ArgumentNullException(nameof(metaProgression));
        _selector = selector ?? throw new ArgumentNullException(nameof(selector));
    }

    /// <summary>
    /// Returns up to 3 unlocked artifacts as a boss drop for the given boss.
    /// Only artifacts that are unlocked in meta-progression can appear.
    /// </summary>
    public ArtifactDefinition[] GetBossDrop(int bossId)
    {
        Log.Info($"[BossDropSystem] Generating drop for boss {bossId}");

        var all = _repository.GetAll();
        var unlocked = new List<ArtifactDefinition>(all.Count);

        for (var i = 0; i < all.Count; i++)
        {
            var artifact = all[i];
            if (_metaProgression.IsUnlocked(artifact.Id))
                unlocked.Add(artifact);
        }

        Log.Info($"[BossDropSystem] Unlocked pool size: {unlocked.Count} / {all.Count}");

        var drop = _selector.Select(unlocked, bossId, DropCount);

        Log.Info($"[BossDropSystem] Drop generated: {drop.Length} artifacts for boss {bossId}");

        return drop;
    }
}
