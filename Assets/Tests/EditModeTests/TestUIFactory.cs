using System.Reflection;
using TMPro;
using UnityEngine;

namespace Tests.EditModeTests.Tests.EditModeTests
{
    public static class TestUIFactory
    {
        public static StatRowUI CreateRowPrefab()
        {
            var go = new GameObject("Row");

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform);
            var label = labelGO.AddComponent<TextMeshProUGUI>();

            var valueGO = new GameObject("Value");
            valueGO.transform.SetParent(go.transform);
            var value = valueGO.AddComponent<TextMeshProUGUI>();

            var row = go.AddComponent<StatRowUI>();

            row.GetType()
                .GetField("nameText", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(row, label);

            row.GetType()
                .GetField("valueText", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(row, value);

            return row;
        }
    }
}