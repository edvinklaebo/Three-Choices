using System;

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

            var instance = artifact.CreateArtifact();
            instance.OnAttach(player);
            player.Artifacts.Add(instance);

            Log.Info($"[ArtifactApplier] Artifact applied: {artifact.DisplayName} to {player.Name}");
        }
    }
}

