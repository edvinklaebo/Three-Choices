# Health Bar System

## Overview

The `HealthBarUI` component is a reusable UI element that displays a Unit's health as a visual bar. It works with any Unit (player, enemy, or any entity with health) and provides smooth visual feedback when health changes.

## Features

- **Normalized Display**: Displays health as a 0-1 fill amount
- **Automatic Updates**: Subscribes to Unit's `HealthChanged` event
- **Smooth Animation**: Uses lerp for smooth health transitions
- **Fully Reusable**: Works with any Unit instance
- **Memory Safe**: Properly manages event subscriptions

## Setup in Unity Editor

### 1. Create Health Bar GameObject

1. Create a new GameObject (e.g., "HealthBar")
2. Add a Canvas (if not already present) and set it as needed
3. Add a Background Image (optional):
   - Add an Image component
   - Set sprite and color for background

4. Add a Fill Image (required):
   - Create a child GameObject called "Fill"
   - Add an Image component
   - Set the Image Type to **Filled**
   - Set Fill Method to **Horizontal**
   - Set sprite and color (e.g., green for health)

### 2. Attach HealthBarUI Component

1. Add the `HealthBarUI` component to your HealthBar GameObject
2. Assign the Fill Image reference in the inspector
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
  - Sets initial fill amount
  - Subscribes to new unit's HealthChanged event

### Inspector Fields

#### `_fillImage` (Image)
The Image component that will be filled based on health percentage. Must have Image Type set to "Filled".

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
```

Run tests via: **Window → General → Test Runner**

## Troubleshooting

### Health Bar Not Updating

1. Check that `Initialize()` was called with a valid Unit
2. Verify the Unit's `HealthChanged` event is being invoked
3. Ensure the Fill Image is assigned in the inspector

### Health Bar Not Visible

1. Check Canvas settings and render mode
2. Verify Fill Image has a sprite and color assigned
3. Ensure Image Type is set to "Filled" with appropriate Fill Method

### Health Bar Jumps Instead of Animating

1. Check that `_lerpSpeed` is set to a reasonable value (1-10)
2. Verify `Update()` is being called (component enabled)

## Integration with Existing Systems

The `HealthBarUI` component integrates seamlessly with the existing Unit system:

- Uses the existing `Unit.HealthChanged` event
- Works with `Unit.ApplyDamage()`, `Unit.Heal()`, and `Unit.ApplyDirectDamage()`
- No modifications to existing code required
- Follows project architectural patterns (thin MonoBehaviour, event-driven)
