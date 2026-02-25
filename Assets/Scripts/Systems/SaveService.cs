using System;
using System.IO;
using UnityEngine;

public static class SaveService
{
    private static string Path =>
        System.IO.Path.Combine(Application.persistentDataPath, "run.json");

    public static void Save(RunState state)
    {
        if (state == null)
        {
            Log.Error("[SaveService] Cannot save null state");
            return;
        }
        
        try
        {
            var json = JsonUtility.ToJson(state, true);
            File.WriteAllText(Path, json);
            Log.Info($"[SaveService] Saved run (Fight: {state.fightIndex}, Player: {state.player?.Name})");
        }
        catch (Exception ex)
        {
            Log.Error($"[SaveService] Failed to save: {ex.Message}");
        }
    }

    public static bool HasSave()
    {
        return File.Exists(Path);
    }

    public static RunState Load()
    {
        if (!HasSave())
        {
            Log.Info("[SaveService] No save file found");
            return null;
        }

        try
        {
            var json = File.ReadAllText(Path);
            var state = JsonUtility.FromJson<RunState>(json);
            
            if (state == null)
            {
                Log.Error("[SaveService] Failed to deserialize save file");
                return null;
            }
            
            // Restore passive event subscriptions and other runtime state after deserialization
            if (state.player != null)
            {
                state.player.RestorePlayerState();
                Log.Info($"[SaveService] Loaded run (Fight: {state.fightIndex}, Player: {state.player.Name}, Passives: {state.player.Passives.Count})");
            }
            else
            {
                Log.Warning("[SaveService] Loaded state has no player data");
            }
            
            return state;
        }
        catch (Exception ex)
        {
            Log.Error($"[SaveService] Failed to load save file: {ex.Message}");
            return null;
        }
    }

    public static void Delete()
    {
        try
        {
            if (HasSave())
            {
                File.Delete(Path);
                Log.Info("[SaveService] Save file deleted");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[SaveService] Failed to delete save file: {ex.Message}");
        }
    }
}