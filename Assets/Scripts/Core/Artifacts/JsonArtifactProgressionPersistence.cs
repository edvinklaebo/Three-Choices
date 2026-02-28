using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Persists artifact unlock state as JSON to <see cref="Application.persistentDataPath"/>.
/// </summary>
public class JsonArtifactProgressionPersistence : IArtifactProgressionPersistence
{
    private const string DefaultFileName = "artifact_progression.json";

    [Serializable]
    private class ArtifactSaveData
    {
        public List<string> unlockedArtifactIds = new();
    }

    private readonly string _savePath;

    public JsonArtifactProgressionPersistence()
    {
        _savePath = Path.Combine(Application.persistentDataPath, DefaultFileName);
    }

    public JsonArtifactProgressionPersistence(string savePath)
    {
        _savePath = savePath ?? throw new ArgumentNullException(nameof(savePath));
    }

    public void Save(IReadOnlyCollection<string> unlockedIds)
    {
        try
        {
            var data = new ArtifactSaveData
            {
                unlockedArtifactIds = new List<string>(unlockedIds)
            };
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_savePath, json);
            Log.Info($"[JsonArtifactProgressionPersistence] Saved {unlockedIds.Count} unlocked artifacts");
        }
        catch (Exception ex)
        {
            Log.Error($"[JsonArtifactProgressionPersistence] Save failed: {ex.Message}");
        }
    }

    public bool Load(out IReadOnlyCollection<string> unlockedIds)
    {
        if (!File.Exists(_savePath))
        {
            Log.Info("[JsonArtifactProgressionPersistence] No save file found");
            unlockedIds = null;
            return false;
        }

        try
        {
            var json = File.ReadAllText(_savePath);
            var data = JsonUtility.FromJson<ArtifactSaveData>(json);

            if (data?.unlockedArtifactIds == null)
            {
                Log.Warning("[JsonArtifactProgressionPersistence] Save file exists but contained no valid data");
                unlockedIds = null;
                return false;
            }

            unlockedIds = data.unlockedArtifactIds;
            Log.Info($"[JsonArtifactProgressionPersistence] Loaded {unlockedIds.Count} unlocked artifacts");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"[JsonArtifactProgressionPersistence] Load failed: {ex.Message}");
            unlockedIds = null;
            return false;
        }
    }

    public void Delete()
    {
        try
        {
            if (File.Exists(_savePath))
                File.Delete(_savePath);
        }
        catch (Exception ex)
        {
            Log.Error($"[JsonArtifactProgressionPersistence] Delete failed: {ex.Message}");
        }
    }
}
