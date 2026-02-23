using System;
using System.Collections.Generic;
using System.Reflection;
using Sentry.Unity;
using UnityEngine;

public static class Log
{
    public static void Info(string message, object data = null)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(Format("INFO", message, data));
#endif
        SentrySdk.AddBreadcrumb(message, "info", data: ToDict(data));
    }

    public static void Warning(string message, object data = null)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning(Format("WARN", message, data));
#endif
        SentrySdk.AddBreadcrumb(message, "warn", data: ToDict(data));
    }

    public static void Error(string message, object data = null)
    {
        Debug.LogError(Format("ERROR", message, data));
        SentrySdk.AddBreadcrumb(message, "error", data: ToDict(data));
    }

    public static void Exception(Exception ex, string context, object data)
    {
        if (!string.IsNullOrEmpty(context))
            SentrySdk.AddBreadcrumb(context, "exception", data: ToDict(data));

        SentrySdk.CaptureException(ex);
    }

    private static string Format(string level, string message, object data)
    {
        return data == null ? $"[{level}] {message}" : $"[{level}] {message} | {data}";
    }

    private static IDictionary<string, string> ToDict(object data)
    {
        if (data == null) return null;

        var dict = new Dictionary<string, string>();
        var type = data.GetType();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead) continue;

            var value = prop.GetValue(data);
            if (value == null) continue;

            dict[prop.Name] = value.ToString();
        }

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = field.GetValue(data);
            if (value == null) continue;

            dict[field.Name] = value.ToString();
        }

        return dict;
    }
}