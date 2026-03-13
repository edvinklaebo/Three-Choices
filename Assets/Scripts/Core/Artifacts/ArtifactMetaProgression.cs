using System;
using System.Collections.Generic;

using Utils;

namespace Core.Artifacts
{
    /// <summary>
    /// Manages unlock state for artifacts across runs (meta-progression).
    /// Artifacts start locked by default and must be unlocked to appear as boss drops.
    /// Persistence is delegated to an injected <see cref="IArtifactProgressionPersistence"/>.
    /// </summary>
    public class ArtifactMetaProgression
    {
        private readonly HashSet<string> _unlockedIds = new();
        private readonly IArtifactProgressionPersistence _persistence;

        public ArtifactMetaProgression(IArtifactProgressionPersistence persistence)
        {
            this._persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        }

        /// <summary>Unlock an artifact so it can appear in boss drops.</summary>
        public void Unlock(string artifactId)
        {
            if (string.IsNullOrEmpty(artifactId))
            {
                Log.Warning("[ArtifactMetaProgression] Cannot unlock artifact with null or empty ID");
                return;
            }

            if (this._unlockedIds.Add(artifactId))
            {
                Log.Info($"[ArtifactMetaProgression] Artifact unlocked: {artifactId}");
                Save();
            }
        }

        /// <summary>Lock an artifact so it cannot appear in boss drops.</summary>
        public void Lock(string artifactId)
        {
            if (this._unlockedIds.Remove(artifactId))
            {
                Log.Info($"[ArtifactMetaProgression] Artifact locked: {artifactId}");
                Save();
            }
        }

        /// <summary>Check whether an artifact is currently unlocked.</summary>
        public bool IsUnlocked(string artifactId)
        {
            return this._unlockedIds.Contains(artifactId);
        }

        /// <summary>Returns a snapshot of all unlocked artifact IDs.</summary>
        public IReadOnlyCollection<string> GetUnlockedIds()
        {
            return this._unlockedIds;
        }

        /// <summary>Saves unlock state via the injected persistence.</summary>
        public void Save()
        {
            this._persistence.Save(this._unlockedIds);
        }

        /// <summary>Loads unlock state via the injected persistence. Returns true if a save was found.</summary>
        public bool Load()
        {
            var found = this._persistence.Load(out var ids);

            if (found && ids != null)
            {
                this._unlockedIds.Clear();
                this._unlockedIds.UnionWith(ids);
            }

            return found;
        }

        /// <summary>Clears all unlocked artifacts and deletes the persisted save.</summary>
        public void Reset()
        {
            this._unlockedIds.Clear();
            this._persistence.Delete();
        }
    }
}

