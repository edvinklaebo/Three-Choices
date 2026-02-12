using UnityEngine;

public class UpgradeListener : MonoBehaviour
{
    [SerializeField] private UpgradeEventChannel upgradePicked;
    [SerializeField] private VoidEventChannel requestNextFight;
    [SerializeField] private RunController runController;

    private void Start()
    {
        runController = FindFirstObjectByType<RunController>();
    }
    
    private void OnEnable()
    {
        upgradePicked.OnRaised += Apply;
    }

    private void OnDisable()
    {
        upgradePicked.OnRaised -= Apply;
    }

    private void Apply(UpgradeDefinition upgrade)
    {
        Log.Info("Applying upgrade ", upgrade);
        
        UpgradeApplier.Apply(upgrade, runController.Player);
        
        requestNextFight.Raise();
    }
}