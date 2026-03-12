using TMPro;

using UnityEngine;

namespace UI.Stats
{
    public class StatRowUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI valueText;

        public void Bind(StatViewData data)
        {
            this.nameText.text = data.Name;
            this.valueText.text = data.Value.ToString();
        }
    }
}