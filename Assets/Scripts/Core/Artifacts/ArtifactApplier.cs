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
                ArtifactId.BerserkerMask  => new BerserkerMask(),
                ArtifactId.BlazingTorch   => new BlazingTorch(),
                ArtifactId.BloodRitual    => new BloodRitual(),
                ArtifactId.CorruptedTome  => new CorruptedTome(),
                ArtifactId.CrownOfEchoes  => new PhantomStrike(),
                ArtifactId.HeartOfOak     => new HeartOfOak(),
                ArtifactId.Hourglass      => new DeathShield(),
                ArtifactId.IronHeart      => new IronHeart(),
                ArtifactId.LuckyHorseshoe => new CritChance(0.1f),
                ArtifactId.PoisonDarts    => new PoisonAmplifier(),
                ArtifactId.PoisonedBlade  => new PoisonAmplifier(),
                ArtifactId.Quickboots     => new Quickboots(),
                ArtifactId.SteelScales    => new SteelScales(),
                ArtifactId.ThornArmor     => new ThornArmor(),
                ArtifactId.TwinBlades     => new TwinBlades(),
                ArtifactId.VampiricFang   => new VampiricFang(),
                ArtifactId.WarGauntlet    => new WarGauntlet(),
                ArtifactId.SpellEcho      => new SpellEcho(),
                _ => throw new ArgumentOutOfRangeException(artifactDefinition.ArtifactId.ToString())
                };

            artifact.OnAttach(unit);
            unit.Artifacts.Add(artifact);

            Log.Info($"[ArtifactApplier] Artifact passive applied: {artifactDefinition.Id} to {unit.Name}");
        }
    }
}

