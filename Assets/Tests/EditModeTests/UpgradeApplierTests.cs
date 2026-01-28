using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;

namespace Tests.EditModeTests
{
public class UpgradeApplierTests
{
    private Unit _unit;

    [SetUp]
    public void Setup()
    {
        _unit = new Unit("Hero");
        _unit.Stats = new Stats
        {
            MaxHP = 100,
            CurrentHP = 50,
            AttackPower = 10,
            Armor = 5,
            Speed = 3
        };
    }

    // ---------- DISPATCH TESTS ----------

    [Test]
    public void Apply_StatUpgrade_DispatchesToStatHandler()
    {
        var upgrade = new UpgradeDefinition
        {
            Type = UpgradeType.Stat,
            Stat = StatType.AttackPower,
            Amount = 5
        };

        UpgradeApplier.Apply(upgrade, _unit);

        Assert.AreEqual(15, _unit.Stats.AttackPower);
    }

    [Test]
    public void Apply_UnknownUpgradeType_Throws()
    {
        var upgrade = new UpgradeDefinition
        {
            Type = (UpgradeType)999
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            UpgradeApplier.Apply(upgrade, _unit));
    }

    // ---------- STAT UPGRADES ----------

    [Test]
    public void ApplyStat_MaxHP_IncreasesMaxAndCurrentHP()
    {
        var upgrade = new UpgradeDefinition
        {
            Type = UpgradeType.Stat,
            Stat = StatType.MaxHP,
            Amount = 20
        };

        UpgradeApplier.Apply(upgrade, _unit);

        Assert.AreEqual(120, _unit.Stats.MaxHP);
        Assert.AreEqual(70, _unit.Stats.CurrentHP);
    }

    [Test]
    public void ApplyStat_AttackPower_IncreasesAttack()
    {
        var upgrade = new UpgradeDefinition
        {
            Type = UpgradeType.Stat,
            Stat = StatType.AttackPower,
            Amount = 7
        };

        UpgradeApplier.Apply(upgrade, _unit);

        Assert.AreEqual(17, _unit.Stats.AttackPower);
    }

    [Test]
    public void ApplyStat_Armor_IncreasesArmor()
    {
        var upgrade = new UpgradeDefinition
        {
            Type = UpgradeType.Stat,
            Stat = StatType.Armor,
            Amount = 4
        };

        UpgradeApplier.Apply(upgrade, _unit);

        Assert.AreEqual(9, _unit.Stats.Armor);
    }

    [Test]
    public void ApplyStat_Speed_IncreasesSpeed()
    {
        var upgrade = new UpgradeDefinition
        {
            Type = UpgradeType.Stat,
            Stat = StatType.Speed,
            Amount = 2
        };

        UpgradeApplier.Apply(upgrade, _unit);

        Assert.AreEqual(5, _unit.Stats.Speed);
    }

    [Test]
    public void ApplyStat_UnknownStat_Throws()
    {
        var upgrade = new UpgradeDefinition
        {
            Type = UpgradeType.Stat,
            Stat = (StatType)999,
            Amount = 10
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            UpgradeApplier.Apply(upgrade, _unit));
    }

    // ---------- ABILITY UPGRADES ----------

    [Test]
    public void ApplyAbility_AlwaysThrows_ForUnknownAbility()
    {
        var upgrade = new UpgradeDefinition
        {
            Type = UpgradeType.Ability,
            AbilityId = "Fireball"
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            UpgradeApplier.Apply(upgrade, _unit));
    }

    // ---------- PASSIVE UPGRADES ----------

    [Test]
    public void ApplyPassive_Thorns_LogsCorrectly()
    {
        var upgrade = new UpgradeDefinition
        {
            Type = UpgradeType.Passive,
            AbilityId = "Thorns"
        };

        LogAssert.Expect(LogType.Log, "Passive Applied: Thorns");

        UpgradeApplier.Apply(upgrade, _unit);
    }

    [Test]
    public void ApplyPassive_Rage_LogsCorrectly()
    {
        var upgrade = new UpgradeDefinition
        {
            Type = UpgradeType.Passive,
            AbilityId = "Rage"
        };

        LogAssert.Expect(LogType.Log, "Passive Applied: Rage");

        UpgradeApplier.Apply(upgrade, _unit);
    }

    [Test]
    public void ApplyPassive_Lifesteal_LogsCorrectly()
    {
        var upgrade = new UpgradeDefinition
        {
            Type = UpgradeType.Passive,
            AbilityId = "Lifesteal"
        };

        LogAssert.Expect(LogType.Log, "Ability Applied: Lifesteal");

        UpgradeApplier.Apply(upgrade, _unit);
    }

    [Test]
    public void ApplyPassive_Poison_LogsCorrectly()
    {
        var upgrade = new UpgradeDefinition
        {
            Type = UpgradeType.Passive,
            AbilityId = "Poison"
        };

        LogAssert.Expect(LogType.Log, "Ability Applied: Poison");

        UpgradeApplier.Apply(upgrade, _unit);
    }

    [Test]
    public void ApplyPassive_UnknownPassive_Throws()
    {
        var upgrade = new UpgradeDefinition
        {
            Type = UpgradeType.Passive,
            AbilityId = "GodMode"
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            UpgradeApplier.Apply(upgrade, _unit));
    }
}
}