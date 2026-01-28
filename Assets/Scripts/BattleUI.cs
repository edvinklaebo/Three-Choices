using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    public Text BattleLog;

    public void Show(string text)
    {
        BattleLog.text = text;
    }
}
