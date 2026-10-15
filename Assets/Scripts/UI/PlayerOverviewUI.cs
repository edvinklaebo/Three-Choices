using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerOverviewUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _content = null;
        [SerializeField] private ScrollRect _scroll = null;

        public void Initialize(TMP_Text content, ScrollRect scroll)
        {
            _content = content != null ? content : throw new ArgumentNullException(nameof(content));
            _scroll = scroll != null ? scroll : throw new ArgumentNullException(nameof(scroll));
            _content.richText = false;
        }

        public void SetText(string text)
        {
            if (_content == null || _scroll == null)
                throw new InvalidOperationException($"PlayerOverviewUI '{name}' must be initialized before use.");

            _content.text = text ?? string.Empty;
            _scroll.StopMovement();
            _scroll.verticalNormalizedPosition = 1f;
        }
    }
}
