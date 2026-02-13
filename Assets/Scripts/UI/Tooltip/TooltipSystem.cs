using System;
using UnityEngine;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem instance;

    public Tooltip tooltip;
    
    public void Awake()
    {
        instance = this;
    }

    public void OnDisable()
    {
        Hide();
    }

    public static void Show(string content, string label)
    {
        if (instance == null || instance.tooltip == null) return;
        
        instance.tooltip.SetText(content, label);
        instance.tooltip.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        if (instance == null || instance.tooltip == null) return;
        
        instance.tooltip.gameObject.SetActive(false);
    }
}
