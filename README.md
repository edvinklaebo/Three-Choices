# Ropa — Roguelike PvE Auto‑Battler 🎯

[![CI](https://github.com/edvinklaebo/ropa/actions/workflows/unity-build.yml/badge.svg)](https://github.com/edvinklaebo/ropa/actions/workflows/unity-build.yml)  
![Unity](https://img.shields.io/badge/unity-6000.3.6f1-blue)

---

## What this project does

**Ropa** is a small Unity roguelike PvE auto‑battler prototype built around a single unit and a 1‑of‑3 draft upgrade system. It demonstrates core game loops (draft → combat → upgrade), simple AI/combat, and an upgrade repository system.

Key folders:
- `Assets/Scenes/` — playable scenes (MainMenu, SplashScreen, DraftScene)
- `Assets/Scripts/` — core gameplay (drafting, combat, upgrades, UI)
- `Assets/Tests/` — EditMode and PlayMode tests

---

## Why this project is useful ✅
- Compact example of an auto‑battler/drafting system for learning and experimentation
- Testable C# code with Unity Test Framework setups for EditMode & PlayMode
- Integrates common Unity packages (Input System, Test Framework) and optional Sentry telemetry

---

## Getting started (for developers) 🔧

### Prerequisites
- Unity Editor matching the project settings: **m_EditorVersion: 6000.3.6f1** (open `ProjectSettings/ProjectVersion.txt`)  
- Git (recommended) and optionally Git LFS for large assets

### Quick start (open and run)
1. Clone the repository:

   ```bash
   git clone https://github.com/<OWNER>/<REPO>.git
   cd <REPO>
   ```

2. Open the project in the Unity Editor (Unity Hub recommended).
3. Open `Assets/Scenes/MainMenu.unity` (or `Assets/Scenes/DraftScene.unity`) and press Play to run in the Editor.

### Build (Editor GUI)
- File → Build Settings → ensure the scenes you want are added → Build.

### Run tests (Editor or CLI)
- From the Editor: Window → General → Test Runner → Run EditMode / PlayMode tests.
- From command line (Unity CLI):

  ```bash
  Unity -projectPath "C:/path/to/repo" -runTests -testPlatform EditMode -testResults "./test-results.xml" -logFile "./editor.log"
  ```

(Refer to Unity docs for the exact CLI flags for your platform/workflow.)

---

## Configuration & integrations ⚙️
- Sentry is included as a package (`io.sentry.unity`) and there are Sentry option assets in `Assets/Plugins/Sentry` and `Assets/Resources/Sentry`. Configure your DSN/project according to your needs.
- Packages are declared in `Packages/manifest.json` (Input System, Test Framework, Visual Scripting, etc.).

---

## Project overview (short) 🗺️
- Draft system: `Assets/Scripts/DraftSystem.cs` and UI in `Assets/Scripts/DraftUI.cs`
- Combat system: `Assets/Scripts/CombatSystem.cs`, `BattleUI.cs`
- Upgrades: `Assets/Scripts/Upgrade*` (definition, repository, applier)
- Helpers & utilities: `GameManager.cs`, `Log.cs`, `SplashAsyncLoader.cs`

---

## Contributing ✨
Thanks for your interest! To contribute:
1. Open an issue to propose changes or report bugs.
2. Fork the repository and create a branch for your change.
3. Add tests for new behaviors where appropriate and ensure existing tests pass.
4. Submit a pull request describing the change and rationale.

Please follow any coding conventions described in `CONTRIBUTING.md` if present — otherwise open an issue to discuss conventions before large refactors.

---

## Where to get help & support 📬
- Open an issue for bugs or feature requests.
- Submit a PR for fixes and improvements.
- For runtime telemetry or crash reports, check Sentry configuration (if enabled) and Sentry docs.

---

## Maintainers & License
- Project appears created locally (user settings indicate `Roguelike PvE auto-battler`).
- There is no top-level `LICENSE` or `CONTRIBUTING.md` in the repository; please add a `LICENSE` file and `CONTRIBUTING.md` to clarify project licensing and contribution details.

---

## Files & workflows to check
- `.github/workflows/unity-build.yml` — GitHub Actions CI build
- `ProjectSettings/ProjectVersion.txt` — required Unity Editor version
- `Assets/Prompt.txt` — short project description: "I’m building a roguelike PvE auto-battler in Unity with a single unit and a 1-of-3 draft upgrade system."

---

If you want, I can also:
- Add a `CONTRIBUTING.md` and `CODE_OF_CONDUCT.md` draft ✅
- Add a LICENSE placeholder (choose from common licenses) ✅
- Add a small `docs/` folder with development notes and architecture diagrams ✅

Would you like me to add any of those files now?