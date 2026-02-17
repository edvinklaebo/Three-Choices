# Modifier System and Meta-Progression

## Overview

The modifier system provides a composable, priority-based approach to applying damage modifications during combat. This system enables rich gameplay mechanics, balancing, and meta-progression without hardcoded logic.

## Core Concepts

### Priority-Based Ordering

Modifiers execute in priority order (lower = earlier):

- **0-99**: Early modifiers (flat bonuses, base stat changes)
- **100-199**: Standard modifiers (most buffs/debuffs)
- **200-299**: Late modifiers (final multipliers, context-sensitive bonuses)
- **300+**: Post-processing (caps, minimums)

### Modifier Application Flow

```
Base Damage → Early Modifiers → Standard Modifiers → Late Modifiers → Post-Processing
```

Example: A unit with 10 base damage, +5 flat bonus, 2x multiplier, and 50% rage:
```
10 (base) 
→ +5 (FlatDamageModifier, priority 10) = 15
→ ×2 (PercentageDamageModifier, priority 100) = 30
→ ×1.5 (Rage, priority 200) = 45
→ crit check (CriticalHitModifier, priority 210)
```

## Available Modifiers

### FlatDamageModifier (Priority: 10)
Adds flat bonus damage to attacks.
```csharp
var modifier = new FlatDamageModifier(attacker, 5); // +5 damage
```

### VulnerabilityModifier (Priority: 50)
Increases damage dealt to a specific target.
```csharp
var modifier = new VulnerabilityModifier(target, 1.5f); // Target takes 50% more damage
```

### PercentageDamageModifier (Priority: 100)
Applies a percentage-based damage multiplier.
```csharp
var modifier = new PercentageDamageModifier(attacker, 1.25f); // +25% damage
```

### Rage (Priority: 200)
Built-in passive. Increases damage based on missing health.
```csharp
attacker.Passives.Add(new Rage(attacker));
```

### ExecuteModifier (Priority: 205)
Bonus damage against low HP targets.
```csharp
var modifier = new ExecuteModifier(attacker, 0.3f, 2.0f); // 2x damage vs targets below 30% HP
```

### CriticalHitModifier (Priority: 210)
Chance-based critical hit damage.
```csharp
var modifier = new CriticalHitModifier(attacker, 0.25f, 2.0f); // 25% crit chance, 2x multiplier
```

## Usage

### Adding Modifiers to Units

**Via Passives (Unit-Specific)**:
```csharp
var rage = new Rage(unit);
unit.Passives.Add(rage);
```

**Via Global Registration**:
```csharp
var modifier = new FlatDamageModifier(attacker, 10);
DamagePipeline.Register(modifier);

// Clean up when done
DamagePipeline.Unregister(modifier);
```

### Creating Custom Modifiers

Implement `IDamageModifier`:

```csharp
public class PiercingModifier : IDamageModifier
{
    public int Priority => 5; // Very early, before armor calculations
    
    private readonly Unit _owner;
    private readonly float _armorIgnore;
    
    public PiercingModifier(Unit owner, float armorIgnore)
    {
        _owner = owner;
        _armorIgnore = Mathf.Clamp01(armorIgnore);
    }
    
    public void Modify(DamageContext ctx)
    {
        if (ctx.Source != _owner) return;
        
        // Increase damage to compensate for ignored armor
        var armorReduction = ctx.Target.Stats.Armor * _armorIgnore;
        var bonus = armorReduction / (100f + ctx.Target.Stats.Armor);
        ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * (1f + bonus));
    }
}
```

## Meta-Progression

The `MetaProgressionSystem` manages persistent modifiers across runs.

### Unlocking Modifiers

```csharp
// Unlock after achieving something
var modifier = new FlatDamageModifier(player, 5);
MetaProgressionSystem.UnlockModifier("meta_flat_damage_1", modifier);
```

### Activating for a Run

```csharp
// At run start
MetaProgressionSystem.ActivateModifier("meta_flat_damage_1");

// At run end
MetaProgressionSystem.DeactivateAll();
```

### Checking Unlocks

```csharp
if (MetaProgressionSystem.IsUnlocked("meta_flat_damage_1"))
{
    // Show in upgrade tree
}

var allUnlocked = MetaProgressionSystem.GetUnlockedModifiers();
```

## Decision Density

The modifier system increases decision density by:

1. **Stacking Interactions**: Different modifiers combine in interesting ways
2. **Priority Awareness**: Players can optimize modifier order
3. **Conditional Triggers**: Modifiers activate based on combat state
4. **Build Variety**: Multiple viable combinations
5. **Risk/Reward**: Late modifiers (crits, execute) vs reliable early modifiers

## Examples

### High-Risk Crit Build
```csharp
player.Passives.Add(new CriticalHitModifier(player, 0.15f, 3.0f)); // 15% chance, 3x damage
player.Passives.Add(new ExecuteModifier(player, 0.3f, 1.5f)); // Finish low HP enemies
```

### Consistent Damage Build
```csharp
DamagePipeline.Register(new FlatDamageModifier(player, 10));
DamagePipeline.Register(new PercentageDamageModifier(player, 1.3f));
```

### Rage-Enhanced Glass Cannon
```csharp
player.Passives.Add(new Rage(player)); // More damage when low HP
player.Passives.Add(new ExecuteModifier(player, 0.5f, 1.5f)); // Execute below 50% HP
```

## Testing

Tests are in `Assets/Tests/EditModeTests/`:
- `ModifierSystemTests.cs` - Core modifier functionality
- `MetaProgressionSystemTests.cs` - Meta-progression features

Run tests via Unity Test Runner or CI pipeline.

## Performance Notes

- Modifiers are sorted once per damage calculation (minimal overhead)
- LINQ usage in hot path is acceptable for small modifier counts (<10)
- For performance-critical scenarios, consider caching sorted modifier lists

## Future Extensions

Potential additions to increase decision density further:

1. **Combo Modifiers**: Trigger after X consecutive hits
2. **Element Types**: Fire, Ice, Lightning with resistances
3. **Conditional Passives**: Active only when conditions met
4. **Modifier Synergies**: Bonus when certain modifiers are combined
5. **Temporary Modifiers**: Buff/debuff system with duration
6. **Target-Specific Modifiers**: Bonus damage vs enemy types
