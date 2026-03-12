using System.Collections.Generic;

using UnityEngine;

namespace UI.Stats
{
    public class StatsPanelUI : MonoBehaviour
    {
        [SerializeField] private StatRowUI rowPrefab;
        [SerializeField] private Transform container;

        private readonly List<StatRowUI> rows = new();

        public void Show(IEnumerable<StatViewData> stats)
        {
            Clear();

            foreach (var stat in stats)
            {
                var row = Instantiate(this.rowPrefab, this.container);
                row.Bind(stat);
                this.rows.Add(row);
            }
        }

        private void Clear()
        {
            foreach (var r in this.rows)
                if (Application.isPlaying)
                    Destroy(r.gameObject);
                else
                    DestroyImmediate(r.gameObject);

            this.rows.Clear();
        }
    }
}