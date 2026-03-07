using System;

public static class ArtifactApplier
{
    /// <summary>
    /// Applies the artifact's effect permanently to the player unit.
    /// Effects persist until game reset (run state cleared).
    /// </summary>
    public static void ApplyToPlayer(ArtifactDefinition artifact, Unit player)
    {
        if (artifact == null) throw new ArgumentNullException(nameof(artifact));
        if (player == null) throw new ArgumentNullException(nameof(player));

        Log.Info($"[ArtifactApplier] Applying artifact '{artifact.DisplayName}' to {player.Name}");

        switch (artifact.EffectType)
        {
            case ArtifactEffectType.AddArtifact:
                ApplyArtifact(artifact, player);
                break;
            default:
                throw new ArgumentOutOfRangeException(artifact.EffectType.ToString());
        }
    }

    private static void ApplyArtifact(ArtifactDefinition artifactDefinition, Unit unit)
    {
        IArtifact artifact = artifactDefinition.Id switch
        {
            "artifact_berserker_mask"     => new NullPassive(),
            "artifact_blazing_torch"      => new NullPassive(),
            "artifact_blood_ritual"       => new NullPassive(),
            "artifact_corrupted_tome"     => new NullPassive(),
            "artifact_crown_of_echoes"    => new PhantomStrike(),
            "artifact_heart_of_oak"       => new NullPassive(),
            "artifact_hourglass"          => new DeathShield(),
            "artifact_iron_heart"         => new NullPassive(),
            "artifact_lucky_horseshoe"    => new CritChance(10f),
            "artifact_poison_darts"       => new PoisonAmplifier(),
            "artifact_poisoned_blade"     => new PoisonAmplifier(),
            "artifact_quickboots"         => new NullPassive(),
            "artifact_steel_scales"       => new NullPassive(),
            "artifact_thorn_armor"        => new NullPassive(),
            "artifact_twin_blades"        => new NullPassive(),
            "artifact_vampiric_fang"      => new NullPassive(),
            "artifact_war_gauntlet"       => new NullPassive(),
            _ => throw new ArgumentOutOfRangeException(artifactDefinition.Id)
        };

        artifact.OnAttach(unit);
        unit.Artifacts.Add(artifact);

        Log.Info($"[ArtifactApplier] Artifact passive applied: {artifactDefinition.Id} to {unit.Name}");
    }
}

