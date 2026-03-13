using System;

using Core.Boss;

using UnityEngine;

namespace Events
{
    [CreateAssetMenu(menuName = "Events/Boss Fight Event")]
    public class BossFightEventChannel : ScriptableObject
    {
        public event Action<BossDefinition> OnRaised;

        public void Raise(BossDefinition boss)
        {
            OnRaised?.Invoke(boss);
        }
    }
}
