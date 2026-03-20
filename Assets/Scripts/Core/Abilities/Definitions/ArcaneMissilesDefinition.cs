using Core;
using Core.Abilities;
using UnityEngine;
using Utils;

namespace Core.Abilities.Definitions
{
    /// <summary>
    ///     Upgrade ScriptableObject for the Arcane Missiles ability.
    ///     Reads base config from the linked <see cref="ArcaneMissilesData"/> asset.
    ///     First pickup creates an <see cref="ArcaneMissiles"/> instance; subsequent pickups
    ///     call <see cref="ArcaneMissiles.AddDamage"/> using <see cref="AbilityData.DamagePerUpgrade"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Upgrades/Arcane Missiles")]
    public class ArcaneMissilesDefinition : AbilityDefinition
    {
        [Tooltip("The data asset that defines Arcane Missiles' base stats and config.")]
        [SerializeField] private ArcaneMissilesData _data;

        public override void Apply(Unit unit)
        {
            Debug.Assert(_data != null, "ArcaneMissilesDefinition: _data must be assigned before calling Apply");
            Log.Info("Ability Applied: Arcane Missiles");

            var existing = FindExistingAbility<ArcaneMissiles>(unit);
            if (existing != null)
                existing.AddDamage(_data.DamagePerUpgrade);
            else
                unit.Abilities.Add(new ArcaneMissiles(_data));
        }

#if UNITY_EDITOR
        public void EditorInit(string identifier, string soName, ArcaneMissilesData data = null)
        {
            id = identifier;
            displayName = soName;
            _data = data;
        }
#endif
    }
}
