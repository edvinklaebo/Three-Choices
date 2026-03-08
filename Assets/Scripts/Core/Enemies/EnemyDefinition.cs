using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     ScriptableObject that describes a type of enemy: its identity, base stats, spawn rules,
///     and any traits that are applied to the runtime unit on creation.
///     Create via the asset menu: Game/Enemies/Enemy Definition.
/// </summary>
[CreateAssetMenu(menuName = "Game/Enemies/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string _enemyName;

    [Header("Base Stats")]
    [SerializeField] private int _maxHP;
    [SerializeField] private int _attackPower;
    [SerializeField] private int _armor;
    [SerializeField] private int _speed;

    [Header("Spawn Rules")]
    [SerializeField] private int _minFightIndex;
    [SerializeField] private int _maxFightIndex = 999;
    /// <summary>Used by future weighted-random spawn system. Currently, all candidates are equally weighted.</summary>
    [SerializeField] private int _spawnWeight = 1;

    [Header("Traits")]
    [SerializeField] private List<EnemyTraitDefinition> _traits = new();

    public string EnemyName => _enemyName;
    public int MaxHP => _maxHP;
    public int AttackPower => _attackPower;
    public int Armor => _armor;
    public int Speed => _speed;
    public int MinFightIndex => _minFightIndex;
    public int MaxFightIndex => _maxFightIndex;
    public int SpawnWeight => _spawnWeight;
    public IReadOnlyList<EnemyTraitDefinition> Traits => _traits;

#if UNITY_EDITOR
    public void EditorInit(string enemyName, int maxHP, int attackPower, int armor, int speed,
        int minFightIndex = 0, int maxFightIndex = 999, int spawnWeight = 1,
        List<EnemyTraitDefinition> traits = null)
    {
        _enemyName = enemyName;
        _maxHP = maxHP;
        _attackPower = attackPower;
        _armor = armor;
        _speed = speed;
        _minFightIndex = minFightIndex;
        _maxFightIndex = maxFightIndex;
        _spawnWeight = spawnWeight;
        _traits = traits ?? new List<EnemyTraitDefinition>();
    }
#endif
}
