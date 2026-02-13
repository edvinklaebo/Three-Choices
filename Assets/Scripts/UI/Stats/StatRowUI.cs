using TMPro;
using UnityEngine;

public class StatRowUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI valueText;

    public void Bind(StatViewData data)
    {
        nameText.text = data.Name;
        valueText.text = data.Value.ToString();
    }
}