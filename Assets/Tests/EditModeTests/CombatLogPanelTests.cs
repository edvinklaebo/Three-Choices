using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.EditModeTests
{
    public class CombatLogPanelTests
    {
        private static CombatLogPanel CreatePanel(int maxEntries = 100)
        {
            var scrollGo = new GameObject("ScrollView");

            var scrollRect = scrollGo.AddComponent<ScrollRect>();

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(scrollGo.transform);
            var content = contentGo.AddComponent<RectTransform>();

            var prefabGo = new GameObject("EntryPrefab");
            prefabGo.transform.SetParent(scrollGo.transform);
            var prefab = prefabGo.AddComponent<TextMeshProUGUI>();

            var panel = scrollGo.AddComponent<CombatLogPanel>();
            SetField(panel, "_scrollRect", scrollRect);
            SetField(panel, "_contentContainer", content);
            SetField(panel, "_entryPrefab", prefab);
            SetField(panel, "_maxEntries", maxEntries);

            return panel;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            target.GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(target, value);
        }

        [Test]
        public void AddEntry_CreatesChildTextObject()
        {
            var panel = CreatePanel();
            var content = (RectTransform)typeof(CombatLogPanel)
                .GetField("_contentContainer", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(panel);

            panel.AddEntry("Test message");

            if (content != null) 
                Assert.AreEqual(1, content.childCount, "One entry should be created");

            Object.DestroyImmediate(panel.gameObject);
        }

        [Test]
        public void AddEntry_SetsCorrectText()
        {
            var panel = CreatePanel();
            var content = (RectTransform)typeof(CombatLogPanel)
                .GetField("_contentContainer", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(panel);

            panel.AddEntry("Hello combat log");

            var text = content?.GetChild(0).GetComponent<TextMeshProUGUI>();
            Assert.IsNotNull(text);
            Assert.AreEqual("Hello combat log", text.text);

            Object.DestroyImmediate(panel.gameObject);
        }

        [Test]
        public void AddEntry_ExceedingMaxEntries_RemovesOldest()
        {
            var panel = CreatePanel(maxEntries: 3);
            _ = (RectTransform)typeof(CombatLogPanel)
                .GetField("_contentContainer", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(panel);

            panel.AddEntry("Entry 1");
            panel.AddEntry("Entry 2");
            panel.AddEntry("Entry 3");
            panel.AddEntry("Entry 4");

            // Destroy is deferred, but in EditMode the queue count reflects the cap immediately
            var entries = (System.Collections.Generic.Queue<TextMeshProUGUI>)typeof(CombatLogPanel)
                .GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(panel);

            if (entries != null) 
                Assert.AreEqual(3, entries.Count, "Queue should be capped at maxEntries");

            Object.DestroyImmediate(panel.gameObject);
        }

        [Test]
        public void Clear_RemovesAllEntries()
        {
            var panel = CreatePanel();

            panel.AddEntry("A");
            panel.AddEntry("B");
            panel.AddEntry("C");

            panel.Clear();

            var entries = (System.Collections.Generic.Queue<TextMeshProUGUI>)typeof(CombatLogPanel)
                .GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(panel);

            if (entries != null) 
                Assert.AreEqual(0, entries.Count, "All entries should be cleared");

            Object.DestroyImmediate(panel.gameObject);
        }

        [Test]
        public void LogAdded_Event_TriggersAddEntry()
        {
            var panel = CreatePanel();
            panel.gameObject.SetActive(true); // triggers OnEnable

            var content = (RectTransform)typeof(CombatLogPanel)
                .GetField("_contentContainer", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(panel);

            // Manually fire the LogAdded event via CombatLogger (simulating a unit event)
            CombatLogger.Instance.LogAdded += panel.AddEntry;

            CombatLogger.Instance.Invoke("Test log entry");

            if (content != null) 
                Assert.AreEqual(1, content.childCount, "Panel should react to LogAdded event");

            CombatLogger.Instance.LogAdded -= panel.AddEntry;
            Object.DestroyImmediate(panel.gameObject);
        }

        [Test]
        public void MultipleEntries_AreAddedInOrder()
        {
            var panel = CreatePanel();
            var content = (RectTransform)typeof(CombatLogPanel)
                .GetField("_contentContainer", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(panel);

            panel.AddEntry("First");
            panel.AddEntry("Second");
            panel.AddEntry("Third");

            if (content != null)
            {
                Assert.AreEqual(3, content.childCount);
                Assert.AreEqual("First", content.GetChild(0).GetComponent<TextMeshProUGUI>().text);
                Assert.AreEqual("Second", content.GetChild(1).GetComponent<TextMeshProUGUI>().text);
                Assert.AreEqual("Third", content.GetChild(2).GetComponent<TextMeshProUGUI>().text);
            }

            Object.DestroyImmediate(panel.gameObject);
        }
    }
}
