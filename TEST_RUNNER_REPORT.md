# Unity EditMode Tests Runner - Discovery Report

## Project Information
- **Project Name**: ROPA (Three Choices - Roguelike PvE Auto-Battler)
- **Unity Version**: 6000.3.6f1
- **Test Framework**: Unity Test Framework v1.6.0
- **Location**: /home/runner/work/Three-Choices/Three-Choices

## Test Structure

### EditMode Tests
- **Location**: `Assets/Tests/EditModeTests/`
- **Assembly Definition**: `EditModeTests.asmdef`
- **Test Count**: ~100+ test files (.cs)
- **Configuration**:
  - Editor-only tests (`includePlatforms: ["Editor"]`)
  - Uses NUnit framework
  - References: UnityEngine.TestRunner, UnityEditor.TestRunner, Unity.TextMeshPro, Ropa
  - Precompiled Reference: nunit.framework.dll

### PlayMode Tests (for reference)
- **Location**: `Assets/Tests/PlayModeTests/`
- **Assembly Definition**: `PlayModeTests.asmdef`

## CI/CD Pipeline

The project uses **GitHub Actions** with the **game-ci/unity-test-runner** (v4) action:
- **Workflow File**: `.github/workflows/unity-tests.yml`
- **EditMode Tests Configuration**:
  - Test Mode: `editmode`
  - Project Path: `.` (root)
  - Artifacts Path: `editmode-results`
- **Requirements**:
  - Unity License credentials (environment variables):
    - `UNITY_LICENSE`
    - `UNITY_EMAIL`
    - `UNITY_PASSWORD`
  - Library caching for faster builds

## How to Run Tests

### Method 1: Using GitHub Actions (Recommended for CI/CD)
The tests are automatically run on:
- **Events**: Push to `develop` branch or Pull Requests to `develop`
- **Workflow**: `.github/workflows/unity-tests.yml`
- **Job**: `editmode-tests`

### Method 2: Using game-ci/unity-test-runner Locally
If you have game-ci installed locally:
```bash
game-ci/unity-test-runner --testMode editmode --projectPath . --artifactsPath ./editmode-results
```

### Method 3: Using Unity Command Line (Requires Unity Installation)
Local Unity installation would need:
```bash
/path/to/Unity -projectPath . -runTests -testMode editmode -testResults ./editmode-results/results.xml
```

### Method 4: Using Rider IDE
- Open the project in JetBrains Rider (IDE Support: com.unity.ide.rider v3.0.38)
- Use the Test Explorer to run EditMode tests directly
- Alternative: Visual Studio (IDE Support: com.unity.ide.visualstudio v2.0.26)

## Current Environment Status

### ✅ Available Tools
- **dotnet**: v10.0.102 ✅
- **Git**: Configured ✅
- **Bash**: Available ✅

### ❌ Missing on This Machine
- **Unity Editor**: Not installed on this runner
- **Unity License**: Not configured (requires secrets)
- **game-ci Docker**: Not available (CI/CD container)

## Test Categories Found

The EditMode tests cover various systems:
- **Combat System**: CombatEngineTests, CombatPhaseTests, CombatSystemTests
- **Status Effects**: StatusEffectTests, PoisonEffectTests, BleedEffectTests, BurnEffectTests
- **Artifacts & Upgrades**: ArtifactApplierTests, UpgradeApplierTests, StatBoostArtifactTests
- **UI Systems**: UIServiceTests, HealthBarUITests, CombatLogPanelTests, DraftUITests
- **Game Flow**: CombatViewPresenterTests, CharacterSelectionTests, IntroSequenceTests
- **Data Systems**: SaveServiceTests, MetaProgressionSystemTests, RunProgressionServiceTests
- **Utilities**: MonsterNameTests, PlatformUtilsTests, RarityRollerTests

## Recommended Actions

### To Run Tests Locally:
1. **Install Unity 6000.3.6f1**
2. **Set up Unity License** (personal or pro)
3. **Open the project** in Unity Editor
4. **Run tests** via Window > General > Test Runner > Edit Mode

### To Run Tests in CI/CD:
1. **Set GitHub Secrets**:
   - `UNITY_LICENSE`
   - `UNITY_EMAIL`
   - `UNITY_PASSWORD`
2. **Push** to `develop` or create a **Pull Request**
3. Tests run automatically via GitHub Actions

## Documentation References

- **GitHub Workflow**: `.github/workflows/unity-tests.yml`
- **Test Framework**: `com.unity.test-framework@1.6.0`
- **Project Settings**: `ProjectSettings/ProjectVersion.txt`
- **Dependencies**: `Packages/manifest.json`
- **IDE Support**: 
  - JetBrains Rider v3.0.38
  - Visual Studio v2.0.26

---

**Last Updated**: 2024
**Status**: Ready for test execution via CI/CD (GitHub Actions)
