using System;
using System.Collections.Generic;

using Core;

using UnityEngine;

namespace Events
{
    [CreateAssetMenu(menuName = "Events/Draft Event")]
    public class DraftEventChannel : ScriptableObject
    {
        public event Action<List<DraftOption>> OnRaised;

        public void Raise(List<DraftOption> draft)
        {
            OnRaised?.Invoke(draft);
        }
    }
}
