using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Tooltip
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string Content;
        public string Label;

        public void OnPointerEnter(PointerEventData eventData)
        {
            TooltipSystem.Show(Content, Label);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipSystem.Hide();
        }
    }
}