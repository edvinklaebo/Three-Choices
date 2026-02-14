using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class StatsPanelTests
    {
        [Test]
        public void Show_CreatesCorrectRowCount()
        {
            var panelGO = new GameObject();
            var container = new GameObject().transform;

            var panel = panelGO.AddComponent<StatsPanelUI>();

            var prefab = TestUIFactory.CreateRowPrefab();

            // inject private fields
            Inject(panel, "rowPrefab", prefab);
            Inject(panel, "container", container);

            var stats = new[]
            {
                new StatViewData("HP", 10),
                new StatViewData("ATK", 5)
            };

            panel.Show(stats);

            Assert.AreEqual(2, container.childCount);
        }

        [Test]
        public void Show_BindsCorrectValues()
        {
            var panelGO = new GameObject();
            var container = new GameObject().transform;

            var panel = panelGO.AddComponent<StatsPanelUI>();
            var prefab = TestUIFactory.CreateRowPrefab();

            Inject(panel, "rowPrefab", prefab);
            Inject(panel, "container", container);

            var stats = new[]
            {
                new StatViewData("Speed", 99)
            };

            panel.Show(stats);

            var row = container.GetChild(0).GetComponent<StatRowUI>();
            var texts = row.GetComponentsInChildren<TextMeshProUGUI>();

            Assert.AreEqual("Speed", texts[0].text);
            Assert.AreEqual("99", texts[1].text);
        }

        [Test]
        public void Show_ClearsPreviousRows()
        {
            var panelGO = new GameObject();
            var container = new GameObject().transform;

            var panel = panelGO.AddComponent<StatsPanelUI>();
            var prefab = TestUIFactory.CreateRowPrefab();

            Inject(panel, "rowPrefab", prefab);
            Inject(panel, "container", container);

            panel.Show(new[] { new StatViewData("HP", 1) });
            panel.Show(new[] { new StatViewData("HP", 2) });

            Assert.AreEqual(1, container.childCount);
        }


        private static void Inject(object obj, string field, object value)
        {
            obj.GetType()
                .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(obj, value);
        }
    }
}