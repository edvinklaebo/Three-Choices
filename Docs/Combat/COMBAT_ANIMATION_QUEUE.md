# Combat Animation Queue System

## Overview

This document describes the implementation of a deterministic animation/action queue system for the turn-based roguelike combat. The system fully decouples combat logic from presentation, ensuring combat remains pure and deterministic while allowing for flexible animation and visual effects.

## Architecture

### High-Level Flow

```
CombatSystem (pure logic)
    ↓
List<ICombatAction>
    ↓
CombatAnimationRunner (MonoBehaviour)
    ↓
AnimationContext (animator, VFX, SFX, UI)
```

## Core Components

### 1. ICombatAction Interface

**Location:** `Assets/Scripts/Interfaces/ICombatAction.cs`

Represents one visualized outcome of combat. All combat actions implement this interface.

```csharp
public interface ICombatAction
{
    IEnumerator Play(AnimationContext ctx);
}
```

**Rules:**
- No gameplay logic inside `Play`
- Only presentation code
- Must always terminate
- Can yield for timing

### 2. AnimationContext

**Location:** `Assets/Scripts/Core/AnimationContext.cs`

Centralized presentation access that keeps actions decoupled from scene objects.

```csharp
public class AnimationContext
{
    public AnimationService Anim { get; set; }
    public UIService UI { get; set; }
    public VFXService VFX { get; set; }
    public SFXService SFX { get; set; }
}
```

### 3. Combat Actions

#### DamageAction
**Location:** `Assets/Scripts/Core/DamageAction.cs`

Represents damage dealt in combat. Plays attack animation, hit animation, and shows damage UI.

```csharp
public class DamageAction : ICombatAction
{
    public Unit Source { get; set; }
    public Unit Target { get; set; }
    public int Amount { get; set; }
}
```

#### DeathAction
**Location:** `Assets/Scripts/Core/DeathAction.cs`

Represents a unit dying. Plays death animation and sound.

```csharp
public class DeathAction : ICombatAction
{
    public Unit Target { get; set; }
}
```

#### StatusEffectAction
**Location:** `Assets/Scripts/Core/StatusEffectAction.cs`

Represents status effect application or damage (poison, burn, bleed, etc.).

```csharp
public class StatusEffectAction : ICombatAction
{
    public Unit Target { get; set; }
    public string EffectName { get; set; }
    public int Amount { get; set; }
}
```

### 4. Service Layer

Placeholder implementations for presentation concerns. Located in `Assets/Scripts/Systems/`:

- **AnimationService** - Unit animations (attack, hit, death)
- **UIService** - UI updates (damage numbers, status icons)
- **VFXService** - Visual effects
- **SFXService** - Sound effects

These services log their actions and yield appropriate timing. They can be replaced with full implementations as needed.

### 5. CombatAnimationRunner

**Location:** `Assets/Scripts/Systems/CombatAnimationRunner.cs`

MonoBehaviour responsible for sequential playback of combat actions.

**Features:**
- Queue management (`Enqueue`, `EnqueueRange`)
- Sequential playback (`PlayAll`)
- Speed multiplier support
- Skip functionality (`SkipAnimations`)
- Safe cancellation (`Cancel`)
- Running state tracking (`IsRunning`)

**Usage Example:**
```csharp
var actions = CombatSystem.RunFight(player, enemy);
runner.EnqueueRange(actions);
runner.PlayAll(animationContext);
yield return new WaitUntil(() => !runner.IsRunning);
```

## Combat System Changes

### Original Behavior
```csharp
public static void RunFight(Unit attacker, Unit defender)
{
    // Combat logic + direct damage application
}
```

### New Behavior
```csharp
public static List<ICombatAction> RunFight(Unit attacker, Unit defender)
{
    var actions = new List<ICombatAction>();
    
    // Combat logic + damage application (unchanged)
    // + action generation for animation
    
    return actions;
}
```

**Key Points:**
- Combat logic remains **pure and deterministic**
- Still applies damage/effects to units (gameplay unchanged)
- **Additionally** generates actions for visualization
- No animation or timing code in combat logic

## Integration

### CombatController Update

**Location:** `Assets/Scripts/Controllers/CombatController.cs`

Updated to use the animation queue system:

1. Creates/finds `CombatAnimationRunner`
2. Initializes `AnimationContext` with services
3. Runs combat logic to get actions
4. Queues actions for animation
5. Waits for animations to complete
6. Continues game flow

```csharp
private IEnumerator StartFightCoroutine(Unit player, int fightIndex)
{
    var enemy = EnemyFactory.Create(fightIndex);
    
    // Run combat logic (pure, deterministic)
    var actions = CombatSystem.RunFight(player, enemy);
    
    // Queue actions for animation
    animationRunner.EnqueueRange(actions);
    animationRunner.PlayAll(_animationContext);
    
    // Wait for animations to complete
    yield return new WaitUntil(() => !animationRunner.IsRunning);
    
    // Continue game flow
    if (player.Stats.CurrentHP <= 0)
        yield break;
        
    fightEnded.Raise();
}
```

## Testing

**Location:** `Assets/Tests/EditModeTests/CombatAnimationSystemTests.cs`

Comprehensive test suite verifies:

1. **Action Generation:** Combat produces correct actions
2. **Action Data:** Actions contain correct source/target/amount data
3. **Determinism:** Combat remains deterministic with action system
4. **State Changes:** Combat still applies damage/effects to units
5. **Death Handling:** Death actions generated correctly
6. **Status Effects:** Status effect actions work with poison/bleed/etc.

## Extending the System

### Adding New Actions

1. Create a new class implementing `ICombatAction`
2. Add presentation logic in the `Play` method
3. Generate the action in `CombatSystem` when appropriate
4. Add tests to verify behavior

Example:
```csharp
public class HealAction : ICombatAction
{
    public Unit Target { get; set; }
    public int Amount { get; set; }
    
    public IEnumerator Play(AnimationContext ctx)
    {
        ctx.UI.ShowHealing(Target, Amount);
        yield return ctx.VFX.PlayHealEffect(Target);
        ctx.SFX.PlayHealSound();
    }
}
```

### Implementing Real Services

Replace placeholder implementations in:
- `AnimationService` - Hook into actual Animator components
- `UIService` - Create floating damage text, update health bars
- `VFXService` - Instantiate particle systems
- `SFXService` - Play AudioClips

## Benefits

1. **Pure Combat Logic:** Combat calculation is now completely separate from presentation
2. **Deterministic:** Same inputs always produce same outputs
3. **Testable:** Can test combat logic without Unity scene
4. **Flexible:** Easy to add new visual effects without touching combat
5. **Replay Support:** Actions can be recorded and replayed
6. **Network Ready:** Actions can be serialized and sent over network
7. **Speed Control:** Built-in support for fast-forward/skip

## Security Summary

CodeQL security scan completed with **0 alerts**. The implementation:
- Uses safe collection types
- Properly handles null checks in critical paths
- Cleans up coroutines on disable
- No user input processing or serialization vulnerabilities
