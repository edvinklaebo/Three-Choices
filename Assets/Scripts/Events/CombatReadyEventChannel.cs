using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Combat Ready Event")]
public class CombatReadyEventChannel : ScriptableObject
{
    public event Action<CombatResult> OnRaised;

    public void Raise(CombatResult result)
    {
        OnRaised?.Invoke(result);
    }
}
