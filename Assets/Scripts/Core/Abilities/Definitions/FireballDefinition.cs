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
            Debug.Assert(_data != null, "FireballDefinition: _data must be assigned before calling Apply");
            Log.Info("Ability Applied: Fireball");

            var existing = FindExistingAbility<Fireball>(unit);
            if (existing != null)
                existing.AddDamage(_data.DamagePerUpgrade);
            else
                unit.Abilities.Add(new Fireball(_data));
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
