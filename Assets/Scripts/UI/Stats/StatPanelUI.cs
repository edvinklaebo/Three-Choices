using System.Collections.Generic;
using UnityEngine;

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
            var row = Instantiate(rowPrefab, container);
            row.Bind(stat);
            rows.Add(row);
        }
    }

    private void Clear()
    {
        foreach (var r in rows)
            if (Application.isPlaying)
                Destroy(r.gameObject);
            else
                DestroyImmediate(r.gameObject);

        rows.Clear();
    }
}