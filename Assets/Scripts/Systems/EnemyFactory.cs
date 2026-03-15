using System;
using System.Collections.Generic;

using Core;

using Interfaces;

using UnityEngine;

using Utils;

namespace Systems
{
    /// <summary>
    ///     ScriptableObject factory that creates enemy <see cref="Unit"/> instances from
    ///     <see cref="EnemyDefinition"/> assets stored in an <see cref="EnemyDatabase"/>.
    ///     Create via the asset menu: Game/Enemy Factory.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Enemy Factory")]
    public class EnemyFactory : ScriptableObject, IEnemyFactory
    {
        [SerializeField] private EnemyPool _pool;

        public Unit Create(int fightIndex)
        {
            try
            {
                var definition = SelectEnemy(fightIndex);
                var unit = CreateFromDefinition(definition);

                Log.Info("Enemy created", new
                {
                    fightIndex,
                    enemyName = definition.EnemyName,
                    maxHP = definition.MaxHP,
                    attackPower = definition.AttackPower,
                    armor = definition.Armor,
                    speed = definition.Speed
                });

                return unit;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "EnemyFactory.Create failed", new { fightIndex });
                throw;
            }
        }

        private EnemyDefinition SelectEnemy(int fightIndex)
        {
            var candidates = new List<EnemyDefinition>();

            for (var i = 0; i < _pool.Enemies.Count; i++)
            {
                var definition = _pool.Enemies[i];
                if (fightIndex >= definition.MinFightIndex && fightIndex <= definition.MaxFightIndex)
                    candidates.Add(definition);
            }

            return WeightedRandom(candidates);
        }

        private static T WeightedRandom<T>(List<T> candidates) where T : EnemyDefinition
        {
            var totalWeight = 0;
            for (var i = 0; i < candidates.Count; i++)
                totalWeight += candidates[i].SpawnWeight;

            var roll = UnityEngine.Random.Range(0, totalWeight);
            var cumulative = 0;
            for (var i = 0; i < candidates.Count; i++)
            {
                cumulative += candidates[i].SpawnWeight;
                if (roll < cumulative)
                    return candidates[i];
            }

            return candidates[candidates.Count - 1];
        }

        private Unit CreateFromDefinition(EnemyDefinition def)
        {
            var unit = new Unit(def.EnemyName)
            {
                Portrait = def.Portrait,
                Stats = new Stats
                {
                    MaxHP = def.MaxHP,
                    CurrentHP = def.MaxHP,
                    AttackPower = def.AttackPower,
                    Armor = def.Armor,
                    Speed = def.Speed
                }
            };

            for (var i = 0; i < def.Traits.Count; i++)
                def.Traits[i].Apply(unit);

            return unit;
        }

#if UNITY_EDITOR
        public void EditorInit(EnemyPool pool)
        {
            _pool = pool;
        }
#endif
    }
}