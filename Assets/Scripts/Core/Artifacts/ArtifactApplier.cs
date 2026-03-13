using System;
using Core.Artifacts.Passives;
using Core.Passives;
using Utils;

namespace Core.Artifacts
{
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
            IArtifact artifact = artifactDefinition.ArtifactId switch
            {
                ArtifactId.BerserkerMask  => new NullPassive(),
                ArtifactId.BlazingTorch   => new NullPassive(),
                ArtifactId.BloodRitual    => new NullPassive(),
                ArtifactId.CorruptedTome  => new NullPassive(),
                ArtifactId.CrownOfEchoes  => new PhantomStrike(),
                ArtifactId.HeartOfOak     => new NullPassive(),
                ArtifactId.Hourglass      => new DeathShield(),
                ArtifactId.IronHeart      => new NullPassive(),
                ArtifactId.LuckyHorseshoe => new CritChance(10f),
                ArtifactId.PoisonDarts    => new PoisonAmplifier(),
                ArtifactId.PoisonedBlade  => new PoisonAmplifier(),
                ArtifactId.Quickboots     => new NullPassive(),
                ArtifactId.SteelScales    => new NullPassive(),
                ArtifactId.ThornArmor     => new NullPassive(),
                ArtifactId.TwinBlades     => new NullPassive(),
                ArtifactId.VampiricFang   => new NullPassive(),
                ArtifactId.WarGauntlet    => new NullPassive(),
                _ => throw new ArgumentOutOfRangeException(artifactDefinition.ArtifactId.ToString())
                };

            artifact.OnAttach(unit);
            unit.Artifacts.Add(artifact);

            Log.Info($"[ArtifactApplier] Artifact passive applied: {artifactDefinition.Id} to {unit.Name}");
        }
    }
}

