using UnityEngine;

public class UIBinder : MonoBehaviour
{
    public DraftUI DraftUI;
    public BattleUI BattleUI;

    private void Awake()
    {
        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.draftUI = DraftUI;
            gm.battleUI = BattleUI;
        }
    }
}