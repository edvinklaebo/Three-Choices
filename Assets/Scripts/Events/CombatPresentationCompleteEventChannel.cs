using System;

using Core;

using UnityEngine;

namespace Events
{
    [CreateAssetMenu(menuName = "Events/Combat Presentation Complete Event")]
    public class CombatPresentationCompleteEventChannel : ScriptableObject
    {
        public event Action<Unit> OnRaised;

        public void Raise(Unit player)
        {
            OnRaised?.Invoke(player);
        }
    }
}
