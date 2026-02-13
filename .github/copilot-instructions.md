# Copilot Coding Agent Instructions

## Project context

- **Engine**: Unity 6.3 LTS
- **Language**: C# (.NET / Unity runtime)
- **Project type**: Game / Roguelike
- **UI**: UGUI + TextMeshPro
- **Tests**: EditMode tests preferred

## Architectural rules (STRICT)

- ❌ No God objects (GameManager must remain thin or orchestration-only)
- ✅ Prefer composition over inheritance
- ✅ Game logic must be decoupled from MonoBehaviours
- ✅ Use events/signals instead of direct system references
- ❌ Do not introduce singletons unless explicitly requested

## Data & state

- ✅ Use ScriptableObjects for configuration/data
- ❌ Do not store mutable runtime state in ScriptableObjects
- ✅ Runtime state belongs in plain C# classes or services
- ✅ Rarity, upgrades, and balance values must be data-driven

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

- Avoid allocations in hot paths
- Avoid LINQ in per-frame or combat loops
- Null-check public/serialized dependencies
- Fail fast with clear errors if setup is invalid

## When generating code

- Favor small, composable classes
- Generate complete implementations, not stubs
- Do not invent systems that do not already exist
- Ask for clarification only if a decision would affect architecture

## When modifying existing code

- Respect existing patterns and naming
- Refactor only when explicitly requested
- Do not rewrite working systems "for cleanliness"

## Out of scope (DO NOT DO)

- No UI styling decisions unless requested
- No balance changes unless requested
- No speculative features
