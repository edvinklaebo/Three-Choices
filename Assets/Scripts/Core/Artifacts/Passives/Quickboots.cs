using System;

using UnityEngine;

using Utils;

namespace Core.Artifacts.Passives
{
    /// <summary>
    /// Quickboots effect.
    /// Permanently increases the owner's speed when attached.
    /// Acts first in combat when speed is equal or higher than the opponent.
    /// </summary>
    [Serializable]
    public class Quickboots : IArtifact
    {
        [SerializeField] private int _speedBonus;

        public int Priority => 100;

        public Quickboots(int speedBonus = 3)
        {
            _speedBonus = speedBonus;
        }

        public void OnAttach(Unit owner)
        {
            owner.Stats.Speed += _speedBonus;

            Log.Info("[Quickboots] Speed increased", new
            {
                unit = owner.Name,
                bonus = _speedBonus,
                newSpeed = owner.Stats.Speed
            });
        }

        public void OnDetach(Unit owner)
        {
            owner.Stats.Speed -= _speedBonus;
        }
    }
}
