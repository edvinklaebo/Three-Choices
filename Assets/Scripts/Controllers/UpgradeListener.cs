using UnityEngine;

public class UpgradeListener : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UpgradeEventChannel upgradePicked;
    [SerializeField] private VoidEventChannel requestNextFight;
    [SerializeField] private FightStartedEventChannel _fightStarted;

    private Unit _player;

    private void OnEnable()
    {
        upgradePicked.OnRaised += Apply;
        if (_fightStarted != null) _fightStarted.OnRaised += OnFightStarted;
    }

    private void OnDisable()
    {
        upgradePicked.OnRaised -= Apply;
        if (_fightStarted != null) _fightStarted.OnRaised -= OnFightStarted;
    }

    private void OnFightStarted(Unit player, int fightIndex)
    {
        _player = player;
    }

    private void Apply(UpgradeDefinition upgrade)
    {
        Log.Info("Applying upgrade ", upgrade);

        // Hide draft UI immediately when upgrade is picked
        if (DraftUI.Instance != null) DraftUI.Instance.Hide(true);

        UpgradeApplier.Apply(upgrade, _player);

        requestNextFight.Raise();
    }
}