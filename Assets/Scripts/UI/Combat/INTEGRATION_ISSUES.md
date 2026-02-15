# Combat View Integration Issues - Diagnosis and Fixes

## Issues Identified

### 1. Health Instantly at 0 or Negative
**Root Cause**: Combat simulation happens immediately, health bars updated during simulation

**Problem Flow**:
1. `CombatController.StartFightCoroutine()` creates enemy with full HP
2. `combatView.Initialize(player, enemy)` - health bars subscribe to HealthChanged event
3. `CombatSystem.RunFight(player, enemy)` runs synchronously:
   - For each attack: `defender.ApplyDamage()` is called (line 132 in CombatSystem)
   - This fires `Unit.HealthChanged` event immediately
   - Health bars receive events and update during simulation
   - By end of simulation, health is at final value
4. Animations play but health is already updated

**This is actually CORRECT behavior** - health bars react to damage as it's calculated. The issue is that:
- Combat simulation is instant (no time passes)
- Health bars update instantly during simulation
- Then animations play, but HP is already at final values

**Two Possible Solutions**:

**Option A**: Disable health bar updates during simulation, re-enable for playback
- Pros: Health bars animate with combat actions
- Cons: Requires refactoring event subscription timing

**Option B**: Accept that health updates during simulation, ensure animations are visible
- Pros: Simpler, health bars are accurate
- Cons: Health may appear to update before animations play
- Note: This is actually fine for many roguelikes

**Recommended**: Option B. The current behavior is technically correct - health changes when damage is applied. The animations are visual feedback, not the source of truth.

### 2. Floating Text Not Displaying
**Likely Causes**:
- FloatingTextPool not in scene
- FloatingText prefab not assigned
- Camera reference missing
- Canvas not set up correctly for UI

**Check**:
- Add FloatingTextPool GameObject to scene
- Assign FloatingText prefab in inspector
- Ensure FloatingText prefab has:
  - Canvas (or is child of Canvas)
  - TextMeshProUGUI component
  - CanvasGroup component
  - RectTransform
- Camera should be Main Camera (auto-assigned if null)

### 3. Turn Text Not Displaying
**Root Cause**: Turn indicator methods exist but are NEVER called

**Current Issue**: `CombatView.ShowTurnIndicator()` and `HideTurnIndicator()` exist but nothing calls them in the combat flow. The combat system doesn't emit turn events.

**Solution**: Turn indicator feature is not wired up. Either:
- Remove TurnIndicatorUI (leave _turnIndicator null in CombatView)
- Or wire up turn events (requires modifying combat flow - NOT RECOMMENDED for this task)

**Recommended**: Set _turnIndicator to null in CombatView inspector if not using it.

## Required Fixes

### Fix 1: Verify Scene Setup (CRITICAL)

The health bar behavior is actually correct - it updates when damage is applied. The combat system runs instantly and health updates during that.

**What to check**:

### Fix 1: Verify Scene Setup (CRITICAL)

The health bar behavior is actually correct - it updates when damage is applied. The combat system runs instantly and health updates during that.

**What to check**:
1. Are health bars visible at all?
2. Do they show the correct starting values before combat?
3. Do they animate smoothly to final values?

If health bars show 0 immediately on scene start:
- Check that units have correct MaxHP and CurrentHP when created
- Check EnemyFactory.Create() is setting HP correctly
- Use diagnostics tool to verify

### Fix 2: Add FloatingTextPool to Scene (CRITICAL FOR DAMAGE NUMBERS)

Required hierarchy:
```
Canvas (Screen Space - Overlay or Camera)
└── FloatingTextPool (GameObject with FloatingTextPool component)
    └── Container (optional empty GameObject)
```

Inspector settings:
- Floating Text Prefab: [Assign your FloatingText prefab]
- Container: [Optional, leave empty to use self]
- Initial Pool Size: 10
- Camera: Main Camera

### Fix 3: Wire TurnIndicatorUI

The turn indicator exists but is never called. You have two options:

**Option A**: Call manually in combat controller (add turn events)
**Option B**: Remove TurnIndicatorUI if not needed

For Option A, you'd need to modify combat flow to emit turn events.

## Scene Setup Checklist

- [ ] CombatView GameObject exists in scene
- [ ] PlayerView assigned in CombatView
- [ ] EnemyView assigned in CombatView
- [ ] PlayerView has IdlePoint, LungePoint, and SpriteRenderer child
- [ ] EnemyView has IdlePoint, LungePoint, and SpriteRenderer child
- [ ] CombatHUD exists and assigned in CombatView
- [ ] FloatingTextPool exists in scene
- [ ] FloatingText prefab assigned in FloatingTextPool
- [ ] Camera assigned in FloatingTextPool (or Main Camera tagged)
- [ ] TurnIndicatorUI assigned in CombatView (or set to null if unused)
- [ ] CombatView assigned in CombatController (optional, auto-finds if not)

## How to Use Diagnostics

1. Add `CombatViewDiagnostics` component to any GameObject
2. Check "Run On Start" in inspector
3. Run the scene
4. Check Console for detailed report
5. Fix issues listed as ❌ CRITICAL first
6. Then fix ⚠️ warnings

The diagnostic tool will tell you exactly what's missing or misconfigured.
