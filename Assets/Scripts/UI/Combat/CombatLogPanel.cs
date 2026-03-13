using System.Collections.Generic;

using Systems;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Utils;

namespace UI.Combat
{
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
            if (this._scrollRect == null) Log.Error("CombatLogPanel: ScrollRect not assigned");
            if (this._contentContainer == null) Log.Error("CombatLogPanel: ContentContainer not assigned");
            if (this._entryPrefab == null) Log.Error("CombatLogPanel: EntryPrefab not assigned");
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
            if (this._entryPrefab == null || this._contentContainer == null)
                return;

            var entry = Instantiate(this._entryPrefab, this._contentContainer);
            entry.text = message;
            this._entries.Enqueue(entry);

            if (this._entries.Count > this._maxEntries)
            {
                var oldest = this._entries.Dequeue();
                DestroyImmediate(oldest.gameObject);
            }

            ScrollToBottom();
        }

        /// <summary>
        ///     Removes all log entries from the scroll view.
        /// </summary>
        public void Clear()
        {
            foreach (var entry in this._entries)
                if (entry != null)
                    DestroyImmediate(entry.gameObject);

            this._entries.Clear();
        }

        private void ScrollToBottom()
        {
            if (this._scrollRect == null)
                return;

            Canvas.ForceUpdateCanvases();
        }
    }
}
