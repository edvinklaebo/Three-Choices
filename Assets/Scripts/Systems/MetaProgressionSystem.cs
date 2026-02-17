using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages persistent modifiers that carry across runs (meta-progression).
/// Modifiers can be unlocked, upgraded, and applied globally.
/// NOTE: Currently stores unlocked modifier IDs only. Actual modifier instances
/// must be recreated on load based on the saved IDs. This is by design to avoid
/// serialization complexity with Unity's JsonUtility.
/// </summary>
public static class MetaProgressionSystem
{
    private static readonly Dictionary<string, IDamageModifier> _unlockedModifiers = new();
    private static readonly List<IDamageModifier> _activeGlobalModifiers = new();
    
    private static string SavePath => 
        Path.Combine(Application.persistentDataPath, "meta_progression.json");

    [System.Serializable]
    private class MetaSaveData
    {
        public List<string> unlockedModifierIds = new();
    }

    /// <summary>
    /// Unlock a persistent modifier that can be applied to future runs.
    /// </summary>
    public static void UnlockModifier(string id, IDamageModifier modifier)
    {
        if (!_unlockedModifiers.ContainsKey(id))
        {
            _unlockedModifiers.Add(id, modifier);

            Log.Info("Meta-progression modifier unlocked", new
            {
                modifierId = id,
                priority = modifier.Priority
            });
            
            // Auto-save after unlocking
            Save();
        }
    }

    /// <summary>
    /// Check if a modifier has been unlocked.
    /// </summary>
    public static bool IsUnlocked(string id)
    {
        return _unlockedModifiers.ContainsKey(id);
    }

    /// <summary>
    /// Apply an unlocked modifier globally for the current run.
    /// </summary>
    public static void ActivateModifier(string id)
    {
        if (_unlockedModifiers.TryGetValue(id, out var modifier))
        {
            DamagePipeline.Register(modifier);
            _activeGlobalModifiers.Add(modifier);

            Log.Info("Meta-progression modifier activated", new
            {
                modifierId = id,
                priority = modifier.Priority
            });
        }
    }

    /// <summary>
    /// Deactivate all global modifiers (typically at run end).
    /// </summary>
    public static void DeactivateAll()
    {
        foreach (var modifier in _activeGlobalModifiers)
            DamagePipeline.Unregister(modifier);

        _activeGlobalModifiers.Clear();
    }

    /// <summary>
    /// Get all unlocked modifier IDs.
    /// </summary>
    public static IEnumerable<string> GetUnlockedModifiers()
    {
        return _unlockedModifiers.Keys;
    }
    
    /// <summary>
    /// Save unlocked modifier IDs to disk.
    /// Note: Only IDs are saved, not the modifier instances themselves.
    /// Game code must recreate modifiers from IDs on load.
    /// </summary>
    public static void Save()
    {
        var saveData = new MetaSaveData
        {
            unlockedModifierIds = _unlockedModifiers.Keys.ToList()
        };
        
        var json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        
        Log.Info("Meta-progression saved", new { modifierCount = saveData.unlockedModifierIds.Count });
    }
    
    /// <summary>
    /// Load unlocked modifier IDs from disk.
    /// Returns the list of IDs that were loaded. Game code must register
    /// the actual modifier instances using these IDs.
    /// </summary>
    public static List<string> Load()
    {
        if (!File.Exists(SavePath))
        {
            Log.Info("No meta-progression save found");
            return new List<string>();
        }
        
        var json = File.ReadAllText(SavePath);
        var saveData = JsonUtility.FromJson<MetaSaveData>(json);
        
        Log.Info("Meta-progression loaded", new { modifierCount = saveData.unlockedModifierIds.Count });
        
        return saveData.unlockedModifierIds ?? new List<string>();
    }
    
    /// <summary>
    /// Register a modifier that was previously unlocked (after loading saved IDs).
    /// Use this to reconnect saved modifier IDs with their instances.
    /// </summary>
    public static void RegisterUnlockedModifier(string id, IDamageModifier modifier)
    {
        if (!_unlockedModifiers.ContainsKey(id))
        {
            _unlockedModifiers.Add(id, modifier);
        }
    }

    /// <summary>
    /// Clear all unlocked modifiers (for testing or reset).
    /// </summary>
    public static void Reset()
    {
        DeactivateAll();
        _unlockedModifiers.Clear();
        
        // Delete save file
        if (File.Exists(SavePath) && Application.isPlaying)
            File.Delete(SavePath);
    }
}
