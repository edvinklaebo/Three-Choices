# Health Bar System

## Overview

The `HealthBarUI` component is a reusable UI element that displays a Unit's health using Unity's Slider component. It works with any Unit (player, enemy, or any entity with health) and provides smooth visual feedback when health changes.

## Features

- **Built on Unity Slider**: Uses Unity's built-in Slider component with all its features
- **Automatic Configuration**: Auto-configures slider settings (min=0, max=1, non-interactable)
- **Automatic Updates**: Subscribes to Unit's `HealthChanged` event
- **Smooth Animation**: Uses lerp for smooth health transitions
- **Fully Reusable**: Works with any Unit instance
- **Memory Safe**: Properly manages event subscriptions

## Setup in Unity Editor

### 1. Create Health Bar GameObject

1. Create a new GameObject (e.g., "HealthBar")
2. Add a Slider component (UI → Slider)
   - The slider will be automatically configured by HealthBarUI
3. Customize the Slider appearance:
   - Style the Background, Fill Area, and Fill images
   - Set colors (e.g., red for background, green for fill)
   - You can remove the Handle if you don't need it

### 2. Attach HealthBarUI Component

1. Add the `HealthBarUI` component to the same GameObject with the Slider
2. Optionally assign the Slider reference in the inspector (or let it auto-find)
3. Optionally adjust the Lerp Speed (default: 5)

### 3. Initialize in Code

```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private HealthBarUI _healthBar;
    private Unit _playerUnit;

    private void Start()
    {
        // Create or get your unit
        _playerUnit = new Unit("Player")
        {
            Stats = new Stats
            {
                MaxHP = 100,
                CurrentHP = 100,
                // ... other stats
            }
        };

        // Initialize the health bar
        _healthBar.Initialize(_playerUnit);
    }
}
```

## API Reference

### Public Methods

#### `void Initialize(Unit unit)`

Initializes the health bar to track the specified unit's health.

- **Parameters**: 
  - `unit`: The Unit to track (cannot be null)
- **Behavior**:
  - Unsubscribes from previous unit if any
  - Sets initial slider value
  - Subscribes to new unit's HealthChanged event

### Inspector Fields

#### `_slider` (Slider)
The Slider component that represents the health bar. Will auto-find if not assigned.

#### `_lerpSpeed` (float)
Speed of the health bar animation. Higher values = faster transitions. Default: 5.0

## Examples

### Example 1: Player Health Bar

```csharp
public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private HealthBarUI _healthBar;
    [SerializeField] private Unit _player;

    private void Start()
    {
        _healthBar.Initialize(_player);
    }
}
```

### Example 2: Enemy Health Bar

```csharp
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private HealthBarUI _healthBarPrefab;
    
    public void SpawnEnemy()
    {
        var enemy = new Unit("Enemy")
        {
            Stats = new Stats { MaxHP = 50, CurrentHP = 50, /* ... */ }
        };
        
        var healthBar = Instantiate(_healthBarPrefab);
        healthBar.Initialize(enemy);
    }
}
```

### Example 3: Switching Between Units

```csharp
// You can re-initialize the same health bar with a different unit
_healthBar.Initialize(newUnit);
// The component automatically unsubscribes from the old unit
```

## Architecture Notes

### Unity Slider Integration

The health bar leverages Unity's built-in Slider component which provides:
- Automatic layout and scaling
- Built-in fill visualization
- Easy customization through inspector
- Standard UI component behavior

### Automatic Configuration

In the `Awake()` method, the component:
- Auto-finds the Slider component if not assigned
- Sets `minValue = 0` and `maxValue = 1`
- Sets `interactable = false` (health bars shouldn't be user-controlled)

### Event-Driven Updates

The health bar uses Unity's event system to listen for health changes:
- Subscribe: When `Initialize()` is called
- Update: When `Unit.HealthChanged` event fires
- Unsubscribe: When component is disabled or re-initialized

### Performance

- No per-frame calculations when health is stable
- Lerp only runs when there's a difference between current and target
- Efficient event-based updates only when health actually changes

### Memory Management

- Properly unsubscribes from events in `OnDisable()`
- Unsubscribes from previous unit when re-initialized
- No memory leaks from event subscriptions

## Testing

EditMode tests are available in `Assets/Tests/EditModeTests/HealthBarUITests.cs`:

```
✓ Initialize with valid unit
✓ Initialize with null unit (logs error)
✓ Health changes update the bar
✓ Healing updates the bar
✓ Multiple health changes work correctly
✓ Zero max HP handled gracefully
✓ Auto-finds Slider component
```

Run tests via: **Window → General → Test Runner**

## Troubleshooting

### Health Bar Not Updating

1. Check that `Initialize()` was called with a valid Unit
2. Verify the Unit's `HealthChanged` event is being invoked
3. Ensure the Slider component exists on the GameObject

### Health Bar Not Visible

1. Check Canvas settings and render mode
2. Verify Slider has Fill Area and Fill image assigned
3. Ensure Fill image has a sprite and color

### Health Bar Jumps Instead of Animating

1. Check that `_lerpSpeed` is set to a reasonable value (1-10)
2. Verify `Update()` is being called (component enabled)

### Slider Not Found Error

1. Make sure a Slider component exists on the same GameObject
2. Or manually assign the Slider reference in the inspector

## Integration with Existing Systems

The `HealthBarUI` component integrates seamlessly with the existing Unit system:

- Uses the existing `Unit.HealthChanged` event
- Works with `Unit.ApplyDamage()`, `Unit.Heal()`, and `Unit.ApplyDirectDamage()`
- No modifications to existing code required
- Follows project architectural patterns (thin MonoBehaviour, event-driven)
