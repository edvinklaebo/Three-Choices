# Status Effect System

A flexible, extensible system for applying status effects to units in combat.

## Overview

The status effect system allows units to have temporary effects that modify their behavior or deal damage over time. Status effects tick at specific points during combat turns and can stack, expire, and have custom behaviors.

## Core Components

### IStatusEffect Interface

Defines the contract for all status effects:

```csharp
public interface IStatusEffect
{
    string Id { get; }          // Unique identifier for the effect
    int Stacks { get; }         // Number of stacks (for stackable effects)
    int Duration { get; }       // Remaining turns
    
    void OnApply(Unit target);      // Called when first applied
    void OnTurnStart(Unit target);  // Called at start of unit's turn
    void OnTurnEnd(Unit target);    // Called at end of unit's turn
    void OnExpire(Unit target);     // Called when duration reaches 0
    void AddStacks(int amount);     // Add more stacks to existing effect
}
```

### Unit Integration

Units now have:
- `List<IStatusEffect> StatusEffects` - Active status effects
- `ApplyStatus(IStatusEffect effect)` - Apply a new effect (stacks if same ID exists)
- `ApplyDirectDamage(int damage)` - Take damage that bypasses armor (used by poison)
- `TickStatusesTurnStart()` - Process effects at turn start
- `TickStatusesTurnEnd()` - Process effects at turn end

### Combat Integration

The combat loop now:
1. Determines whose turn it is
2. **Ticks status effects at turn start** (new)
3. Checks if unit died from status effects (new)
4. Performs attack if unit is still alive
5. Ticks status effects at turn end (new)
6. Switches turns

## Poison Effect

The first implemented status effect.

### Characteristics
- **Stackable**: Multiple applications add stacks
- **Turn-based**: Deals damage at turn start
- **Bypasses armor**: Uses direct damage
- **Can kill**: Poison damage can reduce HP to 0
- **Max stacks**: 999 (no limit enforced in code)

### Usage Examples

```csharp
// Apply poison from a weapon
target.ApplyStatus(new PoisonEffect(2, 3));  // 2 stacks, 3 turns

// Apply poison from an artifact
target.ApplyStatus(new PoisonEffect(1, 2));  // 1 stack, 2 turns

// Apply strong poison from a skill
target.ApplyStatus(new PoisonEffect(5, 4));  // 5 stacks, 4 turns

// Check if unit has poison
bool hasPois = unit.StatusEffects.Exists(e => e.Id == "Poison");

// Get poison stacks
var poison = unit.StatusEffects.Find(e => e.Id == "Poison");
int stacks = poison?.Stacks ?? 0;
```

### Behavior

When poison is applied:
1. If unit already has poison: stacks are added to existing poison
2. If unit doesn't have poison: new poison effect is added
3. Each turn start: deals damage equal to stacks, duration decrements
4. When duration reaches 0: effect is removed

## Design Decisions

### Why Direct Damage?
Poison uses `ApplyDirectDamage()` instead of `ApplyDamage()` to bypass armor. This is a design choice that makes poison more dangerous and distinct from regular attacks.

### Why Stack on Re-application?
When the same effect is applied multiple times, stacks accumulate. This allows:
- Multiple sources of poison to compound
- Poison to become more dangerous over time
- Natural scaling for multi-hit abilities

Alternative design (not implemented): Refresh duration on re-application.

### Why Tick at Turn Start?
Ticking at turn start means:
- Poison can kill before the unit acts
- More dramatic/punishing gameplay
- Matches most roguelike conventions

## Extensibility

The system is designed to support future status effects:

### Potential Effects

**Burn** - Deals damage that decays
```csharp
public class BurnEffect : IStatusEffect
{
    public void OnTurnStart(Unit target)
    {
        target.ApplyDirectDamage(Stacks);
        Stacks--;  // Decay stacks
        Duration--;
    }
}
```

**Bleed** - Deals damage when attacking
```csharp
public class BleedEffect : IStatusEffect
{
    public void OnTurnEnd(Unit target)
    {
        // Triggered after attack
        target.ApplyDirectDamage(Stacks);
        Duration--;
    }
}
```

**Weak** - Reduces damage dealt (use with DamagePipeline)
```csharp
public class WeakEffect : IStatusEffect, IDamageModifier
{
    public void Modify(DamageContext ctx)
    {
        if (ctx.Attacker.StatusEffects.Contains(this))
            ctx.FinalValue = (int)(ctx.FinalValue * 0.75f);
    }
}
```

**Regeneration** - Heals over time
```csharp
public class RegenerationEffect : IStatusEffect
{
    public void OnTurnStart(Unit target)
    {
        target.Heal(Stacks);
        Duration--;
    }
}
```

## Testing

Comprehensive tests are in `StatusEffectTests.cs`:
- Poison damage application
- Duration mechanics
- Stack mechanics
- Expiration behavior
- Kill capability
- Armor bypass
- Combat integration
- Multiple effects coexisting

## Architecture Notes

### Separation of Concerns
- **CombatSystem**: Orchestrates flow, no status logic
- **Unit**: Manages status list, delegates to effects
- **StatusEffects**: Own their behavior, self-contained
- **DamagePipeline**: Separate from status system (though can integrate)

### Events
- Poison uses `ApplyDirectDamage` which triggers `HealthChanged` but not `Damaged`
- This distinction allows artifacts to react differently to poison vs attacks
- Future effects can leverage Unit events (Damaged, HealthChanged, Died)

### No Singleton/Static State
- Status effects are instances, not singletons
- Each unit has its own list
- No global status registry needed

## Known Limitations

1. **No duration refresh on re-application**: Stacks add but duration doesn't refresh
   - Could be added with `RefreshDuration()` method if needed
2. **No max stack limit**: Poison can stack infinitely
   - Could add `MaxStacks` property to IStatusEffect if needed
3. **No visual feedback**: System has no UI integration yet
   - Add events or callbacks for UI to subscribe to

## Future Work

- [ ] Add more status effects (Burn, Bleed, Stun, etc.)
- [ ] Integrate with DamagePipeline for damage modification effects
- [ ] Add UI indicators for active status effects
- [ ] Add status effect icons and tooltips
- [ ] Add artifact/ability integration for applying effects
- [ ] Add status effect resistance stats
- [ ] Add dispel/cleanse mechanics
