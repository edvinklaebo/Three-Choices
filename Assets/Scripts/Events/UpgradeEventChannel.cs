using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Upgrade Event")]
public class UpgradeEventChannel : ScriptableObject
{
    public event Action<UpgradeDefinition> OnRaised;

    public void Raise(UpgradeDefinition upgrade)
    {
        OnRaised?.Invoke(upgrade);
    }
}