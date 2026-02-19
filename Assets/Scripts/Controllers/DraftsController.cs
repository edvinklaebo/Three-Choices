using UnityEngine;

public class DraftController : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UpgradeEventChannel upgradePicked;
    [SerializeField] private VoidEventChannel fightEnded;

    private DraftSystem _draft;

    private void Awake()
    {
        Log.Info($"Awake called on {nameof(DraftController)}");

        _draft = new DraftSystem(
            ScriptableObject.CreateInstance<UpgradePool>());
    }

    private void OnEnable()
    {
        fightEnded.OnRaised += OfferDraft;
    }

    private void OnDisable()
    {
        fightEnded.OnRaised -= OfferDraft;
    }

    private void OfferDraft()
    {
        var draft = _draft.GenerateDraft(3);
        DraftUI.Instance.Show(draft, u => upgradePicked.Raise(u));
    }
}