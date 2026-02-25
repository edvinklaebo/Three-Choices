# Copilot Coding Agent Instructions

## Project context

- **Engine**: Unity 6.3 LTS
- **Language**: C# (.NET / Unity runtime)
- **Project type**: Game / Roguelike
- **UI**: UGUI + TextMeshPro
- **Tests**: EditMode tests preferred

## Architectural rules (STRICT)

- ❌ No God objects, prefer single responsibilty.
- ✅ Prefer composition over inheritance.
- ✅ Game logic must be decoupled from MonoBehaviours
- ✅ Use events/signals instead of direct system references.
- ❌ Do not introduce singletons unless explicitly requested.

## Data & state

- ✅ Use ScriptableObjects for configuration/data.
- ❌ Do not store mutable runtime state in ScriptableObjects.
- ✅ Runtime state belongs in plain C# classes or services.
- ✅ Rarity, upgrades, and balance values must be data-driven.
- ✅ Initialize ALL serialized fields with sensible defaults.
- ✅ Validate critical fields in constructors or Init() methods.
- ❌ Do not create dual-role classes (e.g., class serving as both Passive and StatusEffect).

## Code style & conventions

- **Private fields**: `_camelCase`
- **Public members**: `PascalCase`
- **Serialized fields**:
  ```csharp
  [SerializeField] private Type _field;
  ```
- Keep methods < 30 lines where possible
- Avoid magic numbers — extract constants or data assets

## Unity-specific rules

**MonoBehaviours should:**
- Wire dependencies
- Forward events
- Handle lifecycle (Awake, Start, etc.)

**MonoBehaviours should not:**
- Contain business logic
- Perform heavy calculations

**Event subscriptions:**
- Prefer OnEnable/OnDisable for event subscription

## Testing expectations

- ✅ Write EditMode tests for logic and calculations
- ❌ Avoid PlayMode tests unless explicitly required
- Logic must be testable without Unity scene setup

## Performance & safety

- Avoid allocations in hot paths (per-frame, combat loops)
- Avoid LINQ in hot paths (use for loops instead)
- Avoid reflection in hot paths (logging, serialization)
- Null-check public/serialized dependencies in Awake/Start
- Fail fast with clear errors if setup is invalid
- Use `Debug.Assert()` for pre-conditions in development
- Add context when logging errors (include relevant object IDs/names)

## When generating code

- Favor small, composable classes
- Generate complete implementations, not stubs
- Do not invent systems that do not already exist
- Ask for clarification only if a decision would affect architecture

## When modifying existing code

- Respect existing patterns and naming
- Refactor only when explicitly requested
- Do not rewrite working systems "for cleanliness"

## Dependency injection & services

- ✅ Inject dependencies via constructors or explicit setter methods
- ✅ Validate that all required dependencies are set before use
- ❌ Do not use FindObjectOfType as a fallback for missing references
- ❌ Do not create service instances inside controllers (inject them instead)
- ✅ Use ScriptableObject-based event channels for cross-scene communication
- ❌ Avoid static service classes (prefer dependency injection)
- ✅ Document service ownership (who creates/destroys services?)

## Serialization & save/load

- ✅ Use `[SerializeField]` on private fields that need serialization
- ✅ Use `[NonSerialized]` on event delegates, cached references, and circular refs
- ❌ Do not use `readonly` on fields that need serialization
- ✅ Implement restore methods (e.g., `RestoreState()`) after deserialization
- ✅ Re-subscribe to events after deserialization
- ✅ Validate deserialized data before use
- ❌ Do not recreate objects after loading (breaks references)

## Error handling & debugging

- ✅ Use the Log class (Log.Info, Log.Warning, Log.Error, Log.Exception)
- ✅ Include context in error messages (object names, IDs, state)
- ✅ Use try-catch in factory methods and external integrations
- ✅ Throw exceptions early for invalid state (fail fast)
- ✅ Use Debug.Assert() for development-time invariants
- ❌ Do not swallow exceptions silently
- ❌ Do not log in per-frame code (use conditional compilation or Debug.DrawLine)

## Out of scope (DO NOT DO)

- No UI styling decisions unless requested
- No balance changes unless requested
- No speculative features
- No architecture rewrites unless explicitly requested
