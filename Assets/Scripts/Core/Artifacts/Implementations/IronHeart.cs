using System;

using UnityEngine;

using Utils;

namespace Core.Artifacts.Passives
{
    /// <summary>
    /// Iron Heart effect.
    /// Permanently increases the owner's armor when attached.
    /// Represents the fortitude of iron shielding the heart.
    /// </summary>
    [Serializable]
    public class IronHeart : IArtifact
    {
        [SerializeField] private int _armorBonus;

        public int Priority => 100;

        public IronHeart(int armorBonus = 8)
        {
            _armorBonus = armorBonus;
        }

        public void OnAttach(Unit owner)
        {
            owner.Stats.Armor += _armorBonus;

            Log.Info("[IronHeart] Armor increased", new
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
