using Events;

using Systems;

using UnityEngine;

using Utils;

namespace Controllers
{
    public class DraftController : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private DraftEventChannel _showDraft;
        [SerializeField] private VoidEventChannel fightEnded;

        private DraftSystem _draft;

        private void Awake()
        {
            Log.Info($"Awake called on {nameof(DraftController)}");

            this._draft = new DraftSystem(ScriptableObject.CreateInstance<UpgradePool>());
        }

        private void OnEnable()
        {
            this.fightEnded.OnRaised += OfferDraft;
        }

        private void OnDisable()
        {
            this.fightEnded.OnRaised -= OfferDraft;
        }

        private void OfferDraft()
        {
            var draft = this._draft.GenerateDraft(3);
            this._showDraft.Raise(draft);
        }
    }
}