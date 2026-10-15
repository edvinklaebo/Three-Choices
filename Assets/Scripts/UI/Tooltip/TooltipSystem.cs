using UnityEngine;

namespace UI.Tooltip
{
    public class TooltipSystem : MonoBehaviour
    {
        public static TooltipSystem instance;

        public Tooltip tooltip;

        [System.NonSerialized] private Object _owner;

        public void Awake()
        {
            instance = this;
        }

        public void OnDisable()
        {
            Hide();
        }

        public static void Show(string content, string label, Object owner = null)
        {
            if (instance == null || instance.tooltip == null) return;

            instance._owner = owner;
            instance.tooltip.SetText(content, label);
            instance.tooltip.gameObject.SetActive(true);
        }

        public static void Hide()
        {
            if (instance == null || instance.tooltip == null) return;

            instance._owner = null;
            instance.tooltip.gameObject.SetActive(false);
        }

        public static void Hide(Object owner)
        {
            if (instance == null || instance._owner != owner) return;

            Hide();
        }
    }
}