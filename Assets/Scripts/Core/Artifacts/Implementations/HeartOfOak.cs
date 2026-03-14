using System;

using UnityEngine;

using Utils;

namespace Core.Artifacts.Passives
{
    /// <summary>
    /// Heart of Oak effect.
    /// Permanently increases the owner's maximum and current HP when attached.
    /// Represents ancient natural endurance drawn from an oak's resilience.
    /// </summary>
    [Serializable]
    public class HeartOfOak : IArtifact
    {
        [SerializeField] private int _hpBonus;

        public int Priority => 100;

        public HeartOfOak(int hpBonus = 25)
        {
            _hpBonus = hpBonus;
        }

        public void OnAttach(Unit owner)
        {
            owner.Stats.MaxHP += _hpBonus;
            owner.Stats.CurrentHP += _hpBonus;

            Log.Info("[HeartOfOak] HP increased", new
            {
                unit = owner.Name,
                bonus = _hpBonus,
                newMaxHP = owner.Stats.MaxHP
            });
        }

        public void OnDetach(Unit owner)
        {
            owner.Stats.MaxHP -= _hpBonus;
            owner.Stats.CurrentHP = Math.Min(owner.Stats.CurrentHP, owner.Stats.MaxHP);
        }
    }
}
