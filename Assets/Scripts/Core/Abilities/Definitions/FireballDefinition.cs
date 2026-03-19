using Core;
using Core.Abilities;
using UnityEngine;
using Utils;

namespace Core.Abilities.Definitions
{
    /// <summary>
    ///     Upgrade ScriptableObject for the Fireball ability.
    ///     Reads base config from the linked <see cref="FireballData"/> asset.
    ///     First pickup creates a <see cref="Fireball"/> instance; subsequent pickups
    ///     call <see cref="Fireball.AddDamage"/> using <see cref="AbilityData.DamagePerUpgrade"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Upgrades/Fireball")]
    public class FireballDefinition : AbilityDefinition
    {
        [Tooltip("The data asset that defines Fireball's base stats and config.")]
        [SerializeField] private FireballData _data;

        public override void Apply(Unit unit)
        {
            Log.Info("Ability Applied: Fireball");

            var existing = FindExistingAbility<Fireball>(unit);
            if (existing != null)
            {
                var upgradeAmount = _data != null ? _data.DamagePerUpgrade : Fireball.DamagePerStack;
                existing.AddDamage(upgradeAmount);
            }
            else
            {
                var ability = _data != null ? new Fireball(_data) : new Fireball();
                unit.Abilities.Add(ability);
            }
        }

#if UNITY_EDITOR
        public void EditorInit(string identifier, string soName, FireballData data = null)
        {
            id = identifier;
            displayName = soName;
            _data = data;
        }
#endif
    }
}
