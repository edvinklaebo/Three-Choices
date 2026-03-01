using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Boss Fight Event")]
public class BossFightEventChannel : ScriptableObject
{
    public event Action<BossDefinition> OnRaised;

    public void Raise(BossDefinition boss)
    {
        OnRaised?.Invoke(boss);
    }
}
