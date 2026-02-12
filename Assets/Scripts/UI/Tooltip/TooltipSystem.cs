using UnityEngine;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem instance;

    public Tooltip tooltip;
    
    public void Awake()
    {
        instance = this;
    }

    public static void Show(string content, string label)
    {
        instance.tooltip.SetText(content, label);
        instance.tooltip.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        instance.tooltip.gameObject.SetActive(false);
    }
}
