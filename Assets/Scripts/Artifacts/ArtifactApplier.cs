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

    private static void ApplyArtifact(ArtifactDefinition artifact, Unit unit)
    {
        IPassive passive = artifact.AbilityId switch
        {
            "PhantomStrike"     => new PhantomStrike(),
            "DeathShield"       => new DeathShield(),
            "CritChance"        => new CritChancePassive(artifact.Amount / 100f),
            "PoisonAmplifier"   => new PoisonAmplifier(),
            _ => throw new ArgumentOutOfRangeException(artifact.AbilityId)
        };

        passive.OnAttach(unit);
        unit.Passives.Add(passive);

        Log.Info($"[ArtifactApplier] Artifact passive applied: {artifact.AbilityId} to {unit.Name}");
    }
}

