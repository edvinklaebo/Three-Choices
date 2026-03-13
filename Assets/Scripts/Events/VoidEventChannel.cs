using System;

using UnityEngine;

namespace Events
{
    [CreateAssetMenu(menuName = "Events/Void Event")]
    public class VoidEventChannel : ScriptableObject
    {
        public event Action OnRaised;

        public void Raise()
        {
            OnRaised?.Invoke();
        }
    }
}