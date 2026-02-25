using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Fight Started Event")]
public class FightStartedEventChannel : ScriptableObject
{
    public event Action<Unit, int> OnRaised;

    public void Raise(Unit player, int fightIndex)
    {
        OnRaised?.Invoke(player, fightIndex);
    }
}
