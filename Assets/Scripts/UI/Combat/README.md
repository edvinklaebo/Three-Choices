# Combat View System

## Overview

The Combat View system provides a purely reactive visual representation of combat between two units. It follows a strict separation between game logic (CombatSystem) and presentation (CombatView).

## Architecture

### Core Principle
**The view is PURELY REACTIVE - it contains NO game logic, RNG, or stat calculations.**

- **CombatSystem**: Pure logic, calculates outcomes, emits ICombatAction list
- **CombatView**: Pure presentation, reacts to events and plays animations
- **CombatController**: Orchestrator that connects logic to view

### Component Hierarchy

```
CombatView
├── PlayerView (UnitView)
│   ├── IdlePoint (Transform)
│   └── LungePoint (Transform)
├── EnemyView (UnitView)
│   ├── IdlePoint (Transform)
│   └── LungePoint (Transform)
└── CombatHUD
    ├── PlayerHUD (UnitHUDPanel)
    │   ├── NameText (TextMeshProUGUI)
    │   ├── HealthBar (HealthBarUI + Slider)
    │   ├── HPText (TextMeshProUGUI)
    │   └── StatusEffectPanel
    │       └── IconContainer (for StatusEffectIcon instances)
    └── EnemyHUD (UnitHUDPanel)
        └── [Same as PlayerHUD]
```

## Scene Setup

### 1. CombatView GameObject

Create a root `CombatView` GameObject with the `CombatView` component.

### 2. PlayerView Setup

1. Create a child GameObject named "PlayerView"
2. Add `UnitView` component
3. Create child GameObjects:
   - **IdlePoint**: Empty GameObject at local position (0, 0, 0) - marks rest position
   - **LungePoint**: Empty GameObject at local position (e.g., 2, 0, 0) - marks attack position
   - **SpriteRenderer**: Child GameObject with SpriteRenderer for the player sprite at local (0, 0, 0)
4. Position the PlayerView GameObject in world space (e.g., x: -3, y: 0)
5. **Important**: The sprite will animate in local space relative to PlayerView, so PlayerView stays at its world position
6. **Facing**: Player should face RIGHT (positive X direction)

### 3. EnemyView Setup

1. Create a child GameObject named "EnemyView"
2. Add `UnitView` component
3. Create child GameObjects:
   - **IdlePoint**: Empty GameObject at local position (0, 0, 0) - marks rest position
   - **LungePoint**: Empty GameObject at local position (e.g., -2, 0, 0) - marks attack position (negative because enemy lunges left)
   - **SpriteRenderer**: Child GameObject with SpriteRenderer for the enemy sprite at local (0, 0, 0)
4. Position the EnemyView GameObject in world space (e.g., x: 3, y: 0)
5. **Important**: The sprite will animate in local space relative to EnemyView, so EnemyView stays at its world position
6. **Facing**: Enemy should face LEFT (negative X direction)

### 4. CombatHUD Setup (Canvas)

Create a Canvas GameObject under CombatView:

```
Canvas (Screen Space - Overlay)
└── CombatHUD
    ├── PlayerHUD (Panel)
    │   ├── NameText (TextMeshProUGUI)
    │   ├── HealthBarContainer
    │   │   ├── Slider (with HealthBarUI component)
    │   │   └── HPText (TextMeshProUGUI) - displays "50 / 100"
    │   └── StatusEffectPanel
    │       └── IconContainer (Empty, will be populated at runtime)
    └── EnemyHUD (Panel)
        └── [Same structure as PlayerHUD]
```

#### CombatHUD Component Configuration:
- Assign `PlayerHUD` reference
- Assign `EnemyHUD` reference

#### UnitHUDPanel Configuration (for each HUD):
- `_nameText`: TextMeshProUGUI for unit name
- `_healthBar`: HealthBarUI component on the Slider
- `_hpText`: TextMeshProUGUI for numeric HP display
- `_statusEffectPanel`: StatusEffectPanel component

#### StatusEffectPanel Configuration:
- `_iconPrefab`: Prefab with StatusEffectIcon component
- `_iconContainer`: Transform where icons will spawn
- `_maxVisibleIcons`: Set to 6 (default)

### 5. StatusEffectIcon Prefab

Create a prefab for status effect icons:

```
StatusEffectIcon (Image)
├── _iconImage: The main icon Image
├── _stackText: TextMeshProUGUI for stack count (e.g., "3")
├── _durationText: TextMeshProUGUI for duration (e.g., "2")
└── _durationRing: Image for duration visualization (optional)
```

**Layout**:
- Size: ~40x40 pixels
- Stack text: Bottom-right corner
- Duration text: Top-right corner

### 6. Turn Indicator

Create a simple UI element:

```
TurnIndicatorUI (Panel with CanvasGroup)
└── TurnText (TextMeshProUGUI) - displays "Player's Turn" / "Enemy's Turn"
```

Position: Center-top of screen

### 7. FloatingTextPool Setup

Create a GameObject under CombatView:

```
FloatingTextPool
└── Container (optional, for organization)
```

**FloatingTextPool Configuration**:
- `_floatingTextPrefab`: Assign the FloatingText prefab
- `_container`: Assign Container transform
- `_initialPoolSize`: 10 (default)
- `_camera`: Main Camera reference

#### FloatingText Prefab:

```
FloatingText (Canvas Group)
└── Text (TextMeshProUGUI)
```

**RectTransform**: Anchored to bottom-left (0, 0)
**Size**: 100x50

### 8. Wire CombatController

In your `CombatController` inspector:
- Assign `combatView` reference

## Animation Timings

Default timings (can be adjusted in AnimationService):
- **Lunge Forward**: 0.2 seconds
- **Lunge Return**: 0.2 seconds
- **Hit React**: 0.15 seconds
- **Death**: 0.5 seconds

### Animation Approach

**Important**: The lunge animation moves the **sprite child** in local space, NOT the entire UnitView GameObject.

- **UnitView** (parent): Stays at its world position (e.g., -3,0 for player, 3,0 for enemy)
- **Sprite** (child): Animates in local space between IdlePoint (0,0,0) and LungePoint (e.g., 2,0,0)
- **IdlePoint & LungePoint**: Define local offsets for the sprite animation

This ensures child objects remain at their expected local positions (0,0) even when working with Unity's transform hierarchy.

## Events & Updates

### Automatic Updates

The system automatically updates based on Unit events:

1. **Health Changes**: 
   - `Unit.HealthChanged` → Updates HealthBarUI and HPText
   
2. **Status Effects**: 
   - StatusEffectPanel polls `Unit.StatusEffects` each frame
   - (Future: Could be event-driven)

3. **Damage Numbers**: 
   - Spawned via `UIService.ShowDamage()` during attack animations

### Turn Flow

1. `CombatSystem.RunFight()` returns list of ICombatAction
2. `CombatAnimationRunner` plays each action sequentially
3. Each action uses `AnimationContext` to access services:
   - `AnimationService`: Unit movement and animations
   - `UIService`: Damage numbers
   - `VFXService`: Visual effects (placeholder)
   - `SFXService`: Sound effects (placeholder)

## Status Effect Display

### Icon Layout

Status effects are displayed in a horizontal row with:
- **Icon**: Color-coded by effect type
- **Stack Count**: Bottom-right (if > 1)
- **Duration**: Top-right (in turns)

### Overflow Handling

If more than `maxVisibleIcons` effects are active:
- Display first N icons
- Show "+X" overflow indicator as final icon

### Effect Colors (Placeholder)

Until actual sprites are added:
- **Poison**: Purple `(0.5, 0, 0.8)`
- **Bleed**: Red `(0.8, 0, 0)`
- **Burn**: Orange `(1, 0.5, 0)`
- **Stun**: Yellow `(1, 1, 0)`

## Damage Number Colors

- **Physical Damage**: Red `(1, 0.2, 0.2)`
- **Poison Damage**: Purple `(0.6, 0.2, 0.8)`
- **Bleed Damage**: Dark Red `(0.8, 0, 0)`
- **Burn Damage**: Orange `(1, 0.5, 0)`
- **Healing**: Green `(0.2, 1, 0.2)`

## Integration Checklist

- [ ] Create CombatView hierarchy in scene
- [ ] Set up PlayerView with IdlePoint and LungePoint
- [ ] Set up EnemyView with IdlePoint and LungePoint
- [ ] Create Canvas with CombatHUD
- [ ] Create UnitHUDPanel prefabs for player/enemy
- [ ] Create StatusEffectIcon prefab
- [ ] Create FloatingText prefab
- [ ] Set up FloatingTextPool
- [ ] Wire all references in inspector
- [ ] Assign CombatView to CombatController

## Testing

Run the EditMode tests to verify core functionality:
- `CombatViewTests`: View initialization and references
- `UnitViewTests`: Unit positioning and state
- `StatusEffectPanelTests`: Status effect display logic

## Future Enhancements

- Add actual sprite assets for status effects
- Implement proper animation controllers
- Add camera shake/zoom effects
- Add VFX for attacks and status effects
- Make StatusEffectPanel event-driven instead of polling
- Add tooltip on status effect hover
- Add turn transition animations
