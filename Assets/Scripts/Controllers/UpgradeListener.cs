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
        if (_fightStarted != null) 
            _fightStarted.OnRaised += OnFightStarted;
    }

    private void OnDisable()
    {
        upgradePicked.OnRaised -= Apply;
        if (_fightStarted != null) 
            _fightStarted.OnRaised -= OnFightStarted;
    }

    private void OnFightStarted(Unit player, int fightIndex)
    {
        _player = player;
    }

    private void Apply(UpgradeDefinition upgrade)
    {
        if (DraftUI.Instance)
            DraftUI.Instance.Hide();

        UpgradeApplier.Apply(upgrade, _player);

        requestNextFight.Raise();
    }
}