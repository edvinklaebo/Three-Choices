using System;
using Core.Passives;

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

    private static void ApplyArtifact(ArtifactDefinition artifact, Unit unit)
    {
        IPassive passive = artifact.Id switch
        {
            "artifact_crown_of_echoes"    => new PhantomStrike(),
            "artifact_hourglass"          => new DeathShield(),
            "artifact_poison_darts"       => new PoisonAmplifier(),
            "artifact_arcane_tome"        => new NullPassive(),
            "artifact_berserker_mask"     => new NullPassive(),
            "artifact_blazing_torch"      => new NullPassive(),
            "artifact_blood_ritual"       => new NullPassive(),
            "artifact_heart_of_oak"       => new NullPassive(),
            "artifact_iron_heart"         => new NullPassive(),
            "artifact_lucky_horseshoe"    => new CritChancePassive(10f),
            "artifact_poisoned_blade"     => new NullPassive(),
            "artifact_quickboots"         => new NullPassive(),
            "artifact_steel_scales"       => new NullPassive(),
            "artifact_thorn_armor"        => new NullPassive(),
            "artifact_twin_blades"        => new NullPassive(),
            "artifact_vampiric_fang"      => new NullPassive(),
            "artifact_war_gauntlet"       => new NullPassive(),
            _ => throw new ArgumentOutOfRangeException(artifact.Id)
        };

        passive.OnAttach(unit);
        unit.Passives.Add(passive);

        Log.Info($"[ArtifactApplier] Artifact passive applied: {artifact.Id} to {unit.Name}");
    }
}

