# Modifier System - Quick Start Guide

## TL;DR - 3 Ways to Add Damage Modifiers

### 1. Add to Unit's Passives (Recommended for Character Abilities)

```csharp
// Automatically applied when this unit deals damage
var rage = new Rage(player);
player.Passives.Add(rage);
```

### 2. Register Globally (For Environmental Effects)

```csharp
// Applied to all damage calculations
var globalBuff = new PercentageDamageModifier(player, 1.2f);
DamagePipeline.Register(globalBuff);

// Don't forget to unregister when done!
DamagePipeline.Unregister(globalBuff);
```

### 3. Via Meta-Progression (For Permanent Upgrades)

```csharp
// Unlock once (auto-saves to disk)
var metaMod = new FlatDamageModifier(player, 5);
MetaProgressionSystem.UnlockModifier("permanent_damage_1", metaMod);

// On game start, load saved unlocks
var unlockedIds = MetaProgressionSystem.Load();
foreach (var id in unlockedIds)
{
    var modifier = CreateModifierFromId(id, player); // Your factory method
    MetaProgressionSystem.RegisterUnlockedModifier(id, modifier);
}

// Activate each run
MetaProgressionSystem.ActivateModifier("permanent_damage_1");

// Deactivate at run end
MetaProgressionSystem.DeactivateAll();
```

---

## Available Modifiers Cheat Sheet

| Modifier | Priority | Effect | When to Use |
|----------|----------|--------|-------------|
| **FlatDamageModifier** | 10 | +X flat damage | Early game, consistent boost |
| **VulnerabilityModifier** | 50 | Target takes +X% | Boss fights, focus fire |
| **PercentageDamageModifier** | 100 | +X% damage | General purpose buff |
| **Rage** (built-in) | 200 | More dmg at low HP | Risk/reward builds |
| **ExecuteModifier** | 205 | Bonus vs low HP | Finishing enemies |
| **CriticalHitModifier** | 210 | Crit chance + multiplier | High variance builds |

## 5-Second Examples

### Make Player Deal Double Damage
```csharp
var doubleDamage = new PercentageDamageModifier(player, 2.0f);
DamagePipeline.Register(doubleDamage);
```

### Give 25% Crit Chance with 2x Damage
```csharp
player.Passives.Add(new CriticalHitModifier(player, 0.25f, 2.0f));
```

### Bonus Damage vs Low HP Enemies
```csharp
player.Passives.Add(new ExecuteModifier(player, 0.3f, 1.5f)); // 50% bonus below 30% HP
```

### Add Flat +10 Damage to All Attacks
```csharp
player.Passives.Add(new FlatDamageModifier(player, 10));
```

### Make Enemy Take 50% More Damage
```csharp
var vulnerable = new VulnerabilityModifier(boss, 1.5f);
DamagePipeline.Register(vulnerable);
```

---

## Priority Ranges (What You Need to Know)

Lower priority = executes first

- **0-99**: Flat bonuses (adds before multipliers)
- **100-199**: Standard buffs/debuffs
- **200-299**: Final multipliers and conditional bonuses
- **300+**: Post-processing (rarely needed)

**Key Rule**: Flat bonuses before percentage multipliers = more damage!

```csharp
// Good: +5 then ×2 = (10 + 5) × 2 = 30
var flat = new FlatDamageModifier(player, 5);     // Priority 10
var mult = new PercentageDamageModifier(player, 2.0f);  // Priority 100

// The system automatically applies them in the right order!
```

---

## Common Patterns

### Pattern 1: Stacking Multiple Modifiers
```csharp
// These all stack multiplicatively
player.Passives.Add(new FlatDamageModifier(player, 5));           // Base + 5
player.Passives.Add(new PercentageDamageModifier(player, 1.5f));  // Then × 1.5
player.Passives.Add(new Rage(player));                            // Then more at low HP
player.Passives.Add(new CriticalHitModifier(player, 0.2f, 2.0f)); // Then crit chance
```

### Pattern 2: Temporary Buff System
```csharp
public class BuffSystem
{
    private List<IDamageModifier> _activeBuffs = new();
    
    public void ApplyBuff(IDamageModifier mod, float duration)
    {
        DamagePipeline.Register(mod);
        _activeBuffs.Add(mod);
        StartCoroutine(RemoveAfter(mod, duration));
    }
    
    private IEnumerator RemoveAfter(IDamageModifier mod, float duration)
    {
        yield return new WaitForSeconds(duration);
        DamagePipeline.Unregister(mod);
        _activeBuffs.Remove(mod);
    }
}
```

### Pattern 3: Conditional Modifier
```csharp
public class ConditionalDamageModifier : IDamageModifier
{
    public int Priority => 100;
    private readonly Unit _owner;
    private readonly System.Func<bool> _condition;
    private readonly float _multiplier;
    
    public ConditionalDamageModifier(Unit owner, System.Func<bool> condition, float multiplier)
    {
        _owner = owner;
        _condition = condition;
        _multiplier = multiplier;
    }
    
    public void Modify(DamageContext ctx)
    {
        if (ctx.Source != _owner) return;
        if (_condition())
            ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * _multiplier);
    }
}

// Usage:
var nightBonus = new ConditionalDamageModifier(
    player,
    () => TimeManager.IsNight,
    1.5f  // +50% at night
);
```

---

## Creating Custom Modifiers

### Template: Basic Modifier
```csharp
public class MyCustomModifier : IDamageModifier
{
    public int Priority => 100; // Choose based on when it should apply
    
    private readonly Unit _owner;
    private readonly float _bonusAmount;
    
    public MyCustomModifier(Unit owner, float bonusAmount)
    {
        _owner = owner;
        _bonusAmount = bonusAmount;
    }
    
    public void Modify(DamageContext ctx)
    {
        // Only modify damage from this unit
        if (ctx.Source != _owner) return;
        
        // Apply your modification
        ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * (1f + _bonusAmount));
    }
}
```

### Template: Target-Specific Modifier
```csharp
public class MyTargetModifier : IDamageModifier
{
    public int Priority => 50;
    
    private readonly Unit _affectedTarget;
    
    public MyTargetModifier(Unit target)
    {
        _affectedTarget = target;
    }
    
    public void Modify(DamageContext ctx)
    {
        // Only modify damage TO this target
        if (ctx.Target != _affectedTarget) return;
        
        ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * 1.5f);
    }
}
```

---

## Testing Your Modifier

```csharp
[Test]
public void MyModifier_IncreaseDamageByX()
{
    // Setup
    var attacker = CreateTestUnit("Attacker", hp: 100, attack: 10);
    var defender = CreateTestUnit("Defender", hp: 100, attack: 0);
    
    var modifier = new MyCustomModifier(attacker, 0.5f); // +50%
    DamagePipeline.Register(modifier);
    
    // Act
    var ctx = new DamageContext(attacker, defender, 10);
    DamagePipeline.Process(ctx);
    
    // Assert
    Assert.AreEqual(15, ctx.FinalValue, "Should deal 50% more damage");
    
    // Cleanup
    DamagePipeline.Clear();
}
```

---

## Debugging Tips

### Print Damage Calculation Steps
```csharp
// Add to DamagePipeline.Process() temporarily:
foreach (var mod in allModifiers.OrderBy(m => m.Priority))
{
    var before = ctx.FinalValue;
    mod.Modify(ctx);
    var after = ctx.FinalValue;
    
    if (before != after)
        Debug.Log($"[{mod.Priority}] {mod.GetType().Name}: {before} → {after}");
}
```

### Check Active Modifiers
```csharp
// Count global modifiers (use reflection or add debug method):
Debug.Log($"Active global modifiers: {DamagePipeline.Count}");

// Check unit's passive modifiers:
Debug.Log($"Passive modifiers: {unit.Passives.OfType<IDamageModifier>().Count()}");
```

---

## Performance Notes

- ✅ Modifier sorting is fast (typically <10 modifiers)
- ✅ No allocations per frame (only during damage calculation)
- ✅ LINQ usage is acceptable (not in hot loop)
- ⚠️ Don't register/unregister every frame (cache when possible)

---

## Migration Guide (If You Have Old Code)

### Old Hardcoded Damage
```csharp
// Old way ❌
var damage = attacker.Stats.AttackPower;
if (attacker.Stats.CurrentHP < attacker.Stats.MaxHP / 2)
    damage *= 2; // Rage hardcoded
defender.ApplyDamage(attacker, damage);
```

### New Modifier System
```csharp
// New way ✅
attacker.Passives.Add(new Rage(attacker));

// Combat system handles it automatically
var actions = CombatSystem.RunFight(attacker, defender);
```

---

## Common Mistakes to Avoid

❌ **Forgetting to unregister global modifiers**
```csharp
// Bad - memory leak!
DamagePipeline.Register(modifier);
// ... modifier never removed
```

✅ **Always cleanup**
```csharp
// Good
DamagePipeline.Register(modifier);
// ... use modifier
DamagePipeline.Unregister(modifier);
```

❌ **Modifying damage for wrong unit**
```csharp
// Bad - affects all units!
public void Modify(DamageContext ctx)
{
    ctx.FinalValue *= 2; // Doubles EVERYONE's damage
}
```

✅ **Check source/target**
```csharp
// Good
public void Modify(DamageContext ctx)
{
    if (ctx.Source != _owner) return; // Only this unit
    ctx.FinalValue *= 2;
}
```

❌ **Wrong priority causing incorrect order**
```csharp
// Bad - multiplier before flat bonus
var flat = new FlatDamageModifier(player, 5);    // Priority 10
var mult = new MyModifier(player, 2.0f);         // Priority 5 ← Wrong!
// Result: (10 × 2) + 5 = 25 instead of (10 + 5) × 2 = 30
```

✅ **Use standard priority ranges**
```csharp
// Good
var flat = new FlatDamageModifier(player, 5);    // Priority 10
var mult = new MyModifier(player, 2.0f);         // Priority 100 ✓
// Result: (10 + 5) × 2 = 30
```

---

## Need Help?

- 📖 Full documentation: `MODIFIER_SYSTEM.md`
- 📊 Visual guide: `MODIFIER_SYSTEM_VISUAL.md`
- 🔧 Implementation details: `IMPLEMENTATION_SUMMARY_MODIFIERS.md`
- 💡 More examples: `Assets/Scripts/Core/Modifiers/ModifierExamples.cs`
- ✅ Tests: `Assets/Tests/EditModeTests/ModifierSystemTests.cs`

---

## One-Line Recipes

```csharp
// Add +10 damage
player.Passives.Add(new FlatDamageModifier(player, 10));

// Add +50% damage
player.Passives.Add(new PercentageDamageModifier(player, 1.5f));

// Add 20% crit chance with 2x multiplier
player.Passives.Add(new CriticalHitModifier(player, 0.2f, 2.0f));

// Double damage to boss
DamagePipeline.Register(new VulnerabilityModifier(boss, 2.0f));

// 2x damage vs enemies below 25% HP
player.Passives.Add(new ExecuteModifier(player, 0.25f, 2.0f));

// Rage (built-in, already exists)
player.Passives.Add(new Rage(player));
```

That's it! Start with these simple examples and build up to more complex systems.
