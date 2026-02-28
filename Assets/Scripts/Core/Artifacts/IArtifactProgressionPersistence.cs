using System.Collections.Generic;

/// <summary>
/// Abstracts persistence operations for artifact meta-progression state.
/// </summary>
public interface IArtifactProgressionPersistence
{
    /// <summary>Persist the current set of unlocked artifact IDs.</summary>
    void Save(IReadOnlyCollection<string> unlockedIds);

    /// <summary>
    /// Load previously saved artifact IDs.
    /// Returns true if a save was found; <paramref name="unlockedIds"/> contains the loaded IDs.
    /// Returns false if no save exists; <paramref name="unlockedIds"/> will be null.
    /// </summary>
    bool Load(out IReadOnlyCollection<string> unlockedIds);

    /// <summary>Delete the persisted save data.</summary>
    void Delete();
}
