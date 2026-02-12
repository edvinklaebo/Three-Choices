using System.IO;
using UnityEngine;

public static class SaveService
{
    private static string Path =>
        System.IO.Path.Combine(Application.persistentDataPath, "run.json");

    public static void Save(RunState state)
    {
        var json = JsonUtility.ToJson(state, true);
        File.WriteAllText(Path, json);
    }

    public static bool HasSave()
    {
        return File.Exists(Path);
    }

    public static RunState Load()
    {
        if (!HasSave()) return null;

        var json = File.ReadAllText(Path);
        return JsonUtility.FromJson<RunState>(json);
    }

    public static void Delete()
    {
        if (HasSave())
            File.Delete(Path);
    }
}