using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Draft Event")]
public class DraftEventChannel : ScriptableObject
{
    public event Action<List<UpgradeDefinition>> OnRaised;

    public void Raise(List<UpgradeDefinition> draft)
    {
        OnRaised?.Invoke(draft);
    }
}
