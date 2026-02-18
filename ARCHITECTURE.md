# Save/Load Architecture Documentation

## Overview
This document explains the architecture for serializing and deserializing Unit state in the Three Choices roguelike game.

## Core Concepts

### 1. Unit State Components
A `Unit` has three main serializable collections:
- **Abilities**: Active skills (e.g., Fireball)
- **Passives**: Triggered behaviors (e.g., Thorns, Bleed passive)
- **StatusEffects**: Temporary debuffs/buffs (e.g., Burn, Poison, Bleed effects)

### 2. Serialization Strategy

#### Unity's SerializeReference
We use `[SerializeReference]` on the lists to enable polymorphic serialization:
```csharp
[SerializeReference] public List<IAbility> Abilities = new();
[SerializeReference] public List<Passive> Passives = new();
[SerializeReference] public List<IStatusEffect> StatusEffects = new();
```

This allows Unity's JsonUtility to serialize interface/abstract types by storing type information.

#### Field Serialization
Private fields need `[SerializeField]` to be serialized:
```csharp
[SerializeField] private int passiveStacks;  // WILL serialize
private int stacks;                          // WON'T serialize
```

### 3. Event Handling After Deserialization

**Problem**: Event subscriptions are not serialized (they're delegates, not data).

**Solution**: Restore pattern
1. Mark `Owner` field as `[NonSerialized]` to avoid circular references
2. After loading, call `Unit.RestorePlayerState()` which:
   - Iterates through passives
   - Calls `passive.Attach(this)` to set Owner and subscribe to events

```csharp
public void RestorePlayerState()
{
    foreach (var passive in Passives)
    {
        passive.Attach(this);  // Sets Owner and calls OnAttach()
    }
}
```

### 4. Dual-Role Classes (Current Issue)

Some classes serve dual purposes:
- **Passive role**: Attached to player, triggers when conditions met
- **Status Effect role**: Applied to enemies, ticks each turn

Example: `Bleed` class
- As passive: Listens to `Owner.OnHit`, creates Bleed effects on hit
- As status effect: Applied to enemy, deals damage over time

**Data Fields:**
- Passive mode needs: `passiveStacks`, `passiveDuration`, `passiveBaseDamage`
- Status effect mode needs: `Stacks`, `Duration`, `BaseDamage`

**Critical Bug**: If `passiveBaseDamage` isn't initialized, undefined damage values are passed to status effects!

### 5. Initialization Flow

#### New Game
```
StartNewRun(character)
  ↓
CreatePlayerFromCharacter()
  ↓
InitializePlayer(player)
  ↓
Player.Died += event
```

#### Continue Game
```
ContinueRun()
  ↓
SaveService.Load()
  ↓
player.RestorePlayerState()  // Restores passives
  ↓
InitializePlayer(player)     // Sets up events
  ↓
Player.Died += event
```

**IMPORTANT**: Don't recreate the player after loading! Use the deserialized instance directly.

## Best Practices

### DO:
✅ Use `[SerializeField]` on private fields that need serialization
✅ Use `[NonSerialized]` on circular references (Owner, cached lists)
✅ Initialize ALL fields used in serialization
✅ Call `RestorePlayerState()` after deserialization
✅ Use the loaded player instance directly

### DON'T:
❌ Forget `[SerializeField]` on private fields
❌ Use `readonly` on serialized fields (Unity can't serialize them)
❌ Recreate Unit after loading (breaks passive Owner references)
❌ Assume event subscriptions persist after deserialization
❌ Leave fields uninitialized in constructors

## Future Architecture Recommendations

### Separate Passives and StatusEffects
Current dual-role classes create confusion. Recommended refactor:

**Option A: Factory Pattern**
```csharp
// Passive creates status effects but isn't one itself
public class BleedPassive : Passive
{
    public int Stacks { get; set; }
    public int Duration { get; set; }
    public int BaseDamage { get; set; }
    
    private void OnHit(Unit target)
    {
        target.ApplyStatus(CreateBleedEffect());
    }
    
    private BleedEffect CreateBleedEffect()
    {
        return new BleedEffect(Stacks, Duration, BaseDamage);
    }
}

// Separate, focused status effect class
public class BleedEffect : IStatusEffect
{
    public int Stacks { get; set; }
    public int Duration { get; set; }
    public int BaseDamage { get; set; }
    
    public void OnTurnStart(Unit target)
    {
        target.ApplyDirectDamage(Stacks);
        Duration--;
    }
}
```

**Benefits:**
- Single responsibility per class
- Clearer serialization (each class serializes only relevant data)
- Easier to test and maintain
- No confusion about which fields are used when

## Debugging Tips

### Check Serialization
1. Look at run.json in Application.persistentDataPath
2. Verify "data" objects contain expected fields
3. Check "references" section has type information

### Verify Restoration
1. Enable logging in `RestorePlayerState()`
2. Check that passives are attached after load
3. Verify events fire (e.g., add debug logs in OnHit handlers)

### Common Issues
- **Empty "data": {}** - Missing `[SerializeField]` on private fields
- **Passives don't trigger** - `RestorePlayerState()` not called or Owner not restored
- **Null reference errors** - Unit recreated instead of using loaded instance
- **Wrong damage values** - Constructor parameters not initialized properly
