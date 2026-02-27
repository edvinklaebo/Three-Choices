using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Manages unlock state for artifacts across runs (meta-progression).
/// Artifacts start locked by default and must be unlocked to appear as boss drops.
/// Unlock state is saved to disk and persists between sessions.
/// </summary>
public class ArtifactMetaProgression
{
    private readonly HashSet<string> _unlockedIds = new();

    private readonly string _savePath;

    [Serializable]
    private class ArtifactSaveData
    {
        public List<string> unlockedArtifactIds = new();
    }

    public ArtifactMetaProgression()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "artifact_progression.json");
    }

    public ArtifactMetaProgression(string savePath)
    {
        _savePath = savePath ?? throw new ArgumentNullException(nameof(savePath));
    }

    /// <summary>Unlock an artifact so it can appear in boss drops.</summary>
    public void Unlock(string artifactId)
    {
        if (string.IsNullOrEmpty(artifactId))
        {
            Log.Warning("[ArtifactMetaProgression] Cannot unlock artifact with null or empty ID");
            return;
        }

        if (_unlockedIds.Add(artifactId))
        {
            Log.Info($"[ArtifactMetaProgression] Artifact unlocked: {artifactId}");
            Save();
        }
    }

    /// <summary>Lock an artifact so it cannot appear in boss drops.</summary>
    public void Lock(string artifactId)
    {
        if (_unlockedIds.Remove(artifactId))
        {
            Log.Info($"[ArtifactMetaProgression] Artifact locked: {artifactId}");
            Save();
        }
    }

    /// <summary>Check whether an artifact is currently unlocked.</summary>
    public bool IsUnlocked(string artifactId)
    {
        return _unlockedIds.Contains(artifactId);
    }

    /// <summary>Returns a snapshot of all unlocked artifact IDs.</summary>
    public IReadOnlyCollection<string> GetUnlockedIds()
    {
        return _unlockedIds;
    }

    /// <summary>Saves unlock state to disk.</summary>
    public void Save()
    {
        try
        {
            var data = new ArtifactSaveData
            {
                unlockedArtifactIds = new List<string>(_unlockedIds)
            };
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_savePath, json);
            Log.Info($"[ArtifactMetaProgression] Saved {_unlockedIds.Count} unlocked artifacts");
        }
        catch (Exception ex)
        {
            Log.Error($"[ArtifactMetaProgression] Save failed: {ex.Message}");
        }
    }

    /// <summary>Loads unlock state from disk. Returns true if a save file was found.</summary>
    public bool Load()
    {
        if (!File.Exists(_savePath))
        {
            Log.Info("[ArtifactMetaProgression] No save file found");
            return false;
        }

        try
        {
            var json = File.ReadAllText(_savePath);
            var data = JsonUtility.FromJson<ArtifactSaveData>(json);

            if (data?.unlockedArtifactIds != null)
            {
                _unlockedIds.Clear();
                for (var i = 0; i < data.unlockedArtifactIds.Count; i++)
                    _unlockedIds.Add(data.unlockedArtifactIds[i]);
            }

            Log.Info($"[ArtifactMetaProgression] Loaded {_unlockedIds.Count} unlocked artifacts");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"[ArtifactMetaProgression] Load failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>Clears all unlocked artifacts and deletes the save file.</summary>
    public void Reset()
    {
        _unlockedIds.Clear();

        try
        {
            if (File.Exists(_savePath))
                File.Delete(_savePath);
        }
        catch (Exception ex)
        {
            Log.Error($"[ArtifactMetaProgression] Reset failed to delete save: {ex.Message}");
        }
    }
}
