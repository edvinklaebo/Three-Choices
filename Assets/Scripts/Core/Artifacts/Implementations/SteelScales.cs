using System;

using UnityEngine;

using Utils;

namespace Core.Artifacts.Passives
{
    /// <summary>
    /// Steel Scales effect.
    /// Permanently increases the owner's armor when attached.
    /// Represents the natural plate-like protection of overlapping steel scales.
    /// </summary>
    [Serializable]
    public class SteelScales : IArtifact
    {
        [SerializeField] private int _armorBonus;

        public int Priority => 100;

        public SteelScales(int armorBonus = 5)
        {
            _armorBonus = armorBonus;
        }

        public void OnAttach(Unit owner)
        {
            owner.Stats.Armor += _armorBonus;

            Log.Info("[SteelScales] Armor increased", new
            {
                unit = owner.Name,
                bonus = _armorBonus,
                newArmor = owner.Stats.Armor
            });
        }

        public void OnDetach(Unit owner)
        {
            owner.Stats.Armor -= _armorBonus;
        }
    }
}
