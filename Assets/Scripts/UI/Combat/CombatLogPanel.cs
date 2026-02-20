using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Displays CombatLogger messages inside a Scroll View.
///     Attach to the Scroll View root and assign the content RectTransform,
///     a TextMeshProUGUI entry prefab, and the ScrollRect.
///     Subscribes to CombatLogger.Instance.LogAdded in OnEnable/OnDisable.
/// </summary>
public class CombatLogPanel : MonoBehaviour
{
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _contentContainer;
    [SerializeField] private TextMeshProUGUI _entryPrefab;
    [SerializeField] private int _maxEntries = 100;

    private readonly Queue<TextMeshProUGUI> _entries = new();

    private void Awake()
    {
        if (_scrollRect == null) Log.Error("CombatLogPanel: ScrollRect not assigned");
        if (_contentContainer == null) Log.Error("CombatLogPanel: ContentContainer not assigned");
        if (_entryPrefab == null) Log.Error("CombatLogPanel: EntryPrefab not assigned");
    }

    private void OnEnable()
    {
        CombatLogger.Instance.LogAdded += AddEntry;
    }

    private void OnDisable()
    {
        CombatLogger.Instance.LogAdded -= AddEntry;
    }

    /// <summary>
    ///     Appends a new log entry to the scroll view and scrolls to the bottom.
    ///     Removes the oldest entry if the entry limit is exceeded.
    /// </summary>
    public void AddEntry(string message)
    {
        if (_entryPrefab == null || _contentContainer == null)
            return;

        var entry = Instantiate(_entryPrefab, _contentContainer);
        entry.text = message;
        _entries.Enqueue(entry);

        if (_entries.Count > _maxEntries)
        {
            var oldest = _entries.Dequeue();
            Destroy(oldest.gameObject);
        }

        ScrollToBottom();
    }

    /// <summary>
    ///     Removes all log entries from the scroll view.
    /// </summary>
    public void Clear()
    {
        foreach (var entry in _entries)
            if (entry != null)
                Destroy(entry.gameObject);

        _entries.Clear();
    }

    private void ScrollToBottom()
    {
        if (_scrollRect == null)
            return;

        Canvas.ForceUpdateCanvases();
        _scrollRect.verticalNormalizedPosition = 0f;
    }
}
