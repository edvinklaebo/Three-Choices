<div align="center">

# 🎮 Three Choices
### *Roguelike PvE Auto-Battler*

**Every choice matters. Every battle counts.**

[![CI](https://github.com/edvinklaebo/ropa/actions/workflows/unity-build.yml/badge.svg)](https://github.com/edvinklaebo/ropa/actions/workflows/unity-build.yml)
[![Unity](https://img.shields.io/badge/Unity-6000.3.6f1-blue?logo=unity)](https://unity.com/)
[![Made with C#](https://img.shields.io/badge/Made%20with-C%23-239120?logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/license-MIT-green)](#)

[Features](#-features) • [Quick Start](#-quick-start) • [Architecture](#-architecture) • [Contributing](#-contributing)

</div>

---

## 🎯 What is ROPA?

**ROPA** is a tactical roguelike auto-battler where strategic drafting meets intense combat. Command a single powerful unit, make critical choices from 3 upgrade paths, and watch your champion evolve through procedurally generated battles.

> *"One unit. Three choices. Infinite possibilities."*

### 🌟 Core Gameplay Loop

```
🎴 Draft → ⚔️ Battle → ⬆️ Upgrade → 🔄 Repeat
```

Each run is unique. Each decision shapes your destiny. Will you focus on raw power, defensive fortitude, or tactical synergies?

---

## ✨ Features

<table>
<tr>
<td width="50%">

### 🎲 Dynamic Draft System
- **1-of-3 choice mechanic** for meaningful decisions
- Curated upgrade pools with strategic depth
- Rarity-based progression system

</td>
<td width="50%">

### ⚔️ Auto-Battle Combat
- Real-time combat simulation
- Stat-driven battle resolution
- Multiple passive abilities and effects

</td>
</tr>
<tr>
<td width="50%">

### 🔧 Built for Developers
- **Clean architecture** with testable code
- EditMode & PlayMode test coverage
- Event-driven, decoupled systems

</td>
<td width="50%">

### 🚀 Production Ready
- CI/CD pipeline via GitHub Actions
- Automated deployment to itch.io
- Sentry integration for error tracking

</td>
</tr>
</table>

---

## 🚀 Quick Start

### Prerequisites

- **Unity Editor** `6000.3.6f1` (Unity 6 LTS)
- **Git** with LFS support (recommended)
- Basic knowledge of Unity and C#

### Installation

```bash
# Clone the repository
git clone https://github.com/edvinklaebo/ropa.git
cd ropa

# Open in Unity Hub or Unity Editor
# Load Assets/Scenes/MainMenu.unity and hit Play!
```

### 🎮 Play in Editor

1. **Load Scene**: Open `Assets/Scenes/MainMenu.unity`
2. **Press Play**: Experience the full game loop
3. **Alt. Scene**: Try `Assets/Scenes/DraftScene.unity` to jump straight into drafting

### 🧪 Run Tests

**Via Unity Editor:**
```
Window → General → Test Runner → Run All Tests
```

**Via Command Line:**
```bash
Unity -projectPath "/path/to/ropa" \
      -runTests -testPlatform EditMode \
      -testResults "./test-results.xml" \
      -logFile "./editor.log"
```

### 🔨 Build

```
File → Build Settings → Select Target Platform → Build
```

---

## 🏗️ Architecture

### Project Structure

```
Assets/
├── 🎬 Scenes/          # MainMenu, SplashScreen, DraftScene
├── 📜 Scripts/         # Core game logic
│   ├── Systems/        # CombatSystem, DraftSystem, UpgradeSystem
│   ├── Controllers/    # Game flow and orchestration
│   ├── UI/            # UI controllers and view logic
│   ├── Core/          # Domain models and data structures
│   └── Utils/         # Helper classes and extensions
├── 🧪 Tests/          # EditMode & PlayMode tests
├── 🎨 Art/            # Sprites, icons, visual assets
└── ⚙️ Settings/       # ScriptableObject configurations
```

### Key Systems

| System | Purpose | Location |
|--------|---------|----------|
| **DraftSystem** | Manages upgrade selection & presentation | `Assets/Scripts/Systems/` |
| **CombatSystem** | Handles battle simulation & resolution | `Assets/Scripts/Systems/` |
| **UpgradeRepository** | Stores & provides upgrade definitions | `Assets/Scripts/Core/` |
| **GameManager** | Orchestrates game flow & state | `Assets/Scripts/Controllers/` |

### Tech Stack

<div align="center">

| Technology | Purpose |
|:----------:|:-------:|
| ![Unity](https://img.shields.io/badge/Unity-6.3_LTS-black?logo=unity) | Game Engine |
| ![C#](https://img.shields.io/badge/C%23-.NET-239120?logo=c-sharp) | Programming Language |
| ![TextMeshPro](https://img.shields.io/badge/TextMeshPro-UI-blue) | Typography |
| ![Sentry](https://img.shields.io/badge/Sentry-Monitoring-362D59?logo=sentry) | Error Tracking |
| ![GitHub Actions](https://img.shields.io/badge/GitHub_Actions-CI/CD-2088FF?logo=github-actions) | Automation |

</div>

---

## 🎨 Game Design Philosophy

### Core Principles

- **Meaningful Choices**: Every decision should feel impactful
- **Readable Combat**: Players should understand what's happening
- **Iterative Progression**: Rapid prototyping and refinement
- **Data-Driven Design**: Balance via ScriptableObjects, not hardcoded values

### Design Patterns Used

- ✅ **Composition over Inheritance**
- ✅ **Event-Driven Architecture** (decoupled systems)
- ✅ **Repository Pattern** (upgrade management)
- ✅ **Dependency Injection** (via constructor/inspector)
- ❌ **No God Objects** (no monolithic GameManager)
- ❌ **No Singletons** (unless absolutely necessary)

---

## 🤝 Contributing

We welcome contributions! Whether you're fixing bugs, adding features, or improving documentation, your help makes ROPA better.

### How to Contribute

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. **Write** tests for new functionality
4. **Ensure** all tests pass
5. **Commit** your changes (`git commit -m 'Add AmazingFeature'`)
6. **Push** to the branch (`git push origin feature/AmazingFeature`)
7. **Open** a Pull Request

### Development Guidelines

- Follow existing code style and naming conventions
- Write EditMode tests for game logic
- Keep MonoBehaviours thin (orchestration only)
- Document public APIs and complex algorithms
- Avoid introducing new dependencies unnecessarily

---

## 📝 Configuration

### Sentry Setup (Optional)

Sentry is pre-configured for error tracking and analytics:

1. Add your DSN to `Assets/Resources/Sentry/SentryOptions`
2. Configure project settings in `Assets/Plugins/Sentry/`
3. Check [Sentry Unity docs](https://docs.sentry.io/platforms/unity/) for advanced setup

### Package Management

All dependencies are managed via Unity Package Manager (`Packages/manifest.json`):
- Unity Input System
- Unity Test Framework
- Visual Scripting
- TextMeshPro

---

## 🆘 Support & Community

- 🐛 **Bug Reports**: [Open an issue](https://github.com/edvinklaebo/ropa/issues)
- 💡 **Feature Requests**: [Start a discussion](https://github.com/edvinklaebo/ropa/discussions)
- 🔧 **Pull Requests**: Always welcome!
- 📧 **Questions**: Check existing issues or create a new one

---

## 📜 License & Credits

**ROPA** is an open-source project. Please add a `LICENSE` file to clarify usage terms.

### Acknowledgments

- Built with [Unity](https://unity.com/)
- Error tracking by [Sentry](https://sentry.io/)
- Inspired by roguelikes and auto-battlers everywhere

---

<div align="center">

**⭐ Star this repo if you find it useful!**

Made with ❤️ and ☕ by the ROPA team

[Report Bug](https://github.com/edvinklaebo/ropa/issues) • [Request Feature](https://github.com/edvinklaebo/ropa/issues) • [Contribute](CONTRIBUTING.md)

</div>
