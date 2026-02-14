# Character Selector System - Setup Guide

## Overview
This implementation adds a Character Selector system to the game, allowing players to choose between 3 starter characters (Bob, Garry, Steve) before beginning a new run.

## System Components

### 1. Data Models
- **CharacterDefinition.cs**: ScriptableObject defining a character's stats and portrait
- **CharacterDatabase.cs**: ScriptableObject holding all available characters

### 2. Character Selection
- **CharacterSelectController.cs**: Manages character cycling and selection logic
- **CharacterSelectView.cs**: Displays character info and plays selection animations
- **CharacterSelectInput.cs**: Handles keyboard (← → Enter) and mouse input

### 3. Events
- **GameEvents.cs**: Static events for character selection flow
  - `NewRunRequested_Event`: Fired when New Game is pressed
  - `CharacterSelected_Event`: Fired when character is confirmed

### 4. Integration
- **RunController.cs**: Now has `StartNewRun(CharacterDefinition)` overload
- **MainMenuController.cs**: New Game button triggers character selection

## Setup Instructions

### Step 1: Create Character Database Asset
1. Right-click in Project window → Create → Game → CharacterDatabase
2. Name it "CharacterDatabase"
3. Keep it somewhere accessible (e.g., Assets/Resources/)

### Step 2: Create Character Definitions
For each character (Bob, Garry, Steve):
1. Right-click in Project window → Create → Game → Character
2. Set the properties:

**Bob:**
- Id: "bob"
- DisplayName: "Bob"
- MaxHp: 100
- Attack: 10
- Armor: 5
- Speed: 10
- Portrait: Assign `Assets/Art/Characters/bob_placeholder.png`

**Garry:**
- Id: "garry"
- DisplayName: "Garry"
- MaxHp: 120
- Attack: 8
- Armor: 8
- Speed: 8
- Portrait: Assign `Assets/Art/Characters/garry_placeholder.png`

**Steve:**
- Id: "steve"
- DisplayName: "Steve"
- MaxHp: 80
- Attack: 12
- Armor: 3
- Speed: 12
- Portrait: Assign `Assets/Art/Characters/steve_placeholder.png`

3. Add all three characters to the CharacterDatabase's "Characters" list

### Step 3: Setup CharacterSelectScene
1. Open `Assets/Scenes/CharacterSelectScene.unity`
2. Create a Canvas (UI → Canvas)
3. Add the following UI elements:
   - **Character Portrait**: Image component for character sprite
   - **Character Name**: TextMeshPro - Text for displaying name
   - **Stats Text**: TextMeshPro - Text for displaying stats
   - **Previous Button**: Button with "←" or "Previous"
   - **Next Button**: Button with "→" or "Next"
   - **Confirm Button**: Button with "Select" or "Confirm"

4. Create an empty GameObject named "CharacterSelectManager"
5. Add these components to it:
   - CharacterSelectController
   - CharacterSelectView
   - CharacterSelectInput

6. Configure CharacterSelectController:
   - Assign the CharacterDatabase asset
   - Assign the CharacterSelectView component

7. Configure CharacterSelectView:
   - Assign Portrait Image
   - Assign Name Text
   - Assign Stats Text
   - Assign Portrait Transform (for animation)

8. Configure CharacterSelectInput:
   - Assign the CharacterSelectController component

9. Wire up the buttons:
   - Previous Button → OnClick → CharacterSelectInput.OnPreviousClicked()
   - Next Button → OnClick → CharacterSelectInput.OnNextClicked()
   - Confirm Button → OnClick → CharacterSelectInput.OnConfirmClicked()

### Step 4: Setup MainMenu
1. Open `Assets/Scenes/MainMenu.unity`
2. Find the MainMenuController GameObject
3. Add a "New Game" button to the UI
4. Assign the button reference to MainMenuController's "New Game Button" field
5. The button is automatically wired to OnNewGamePressed()

### Step 5: Ensure RunController Exists
The RunController should be present in your scenes. It's marked as DontDestroyOnLoad, so it persists across scenes.

## Flow Summary
```
MainMenu
  ↓ (New Game button pressed)
GameEvents.NewRunRequested_Event fired
  ↓
Load CharacterSelectScene
  ↓
Player navigates with ← → keys or mouse
  ↓
Player presses Enter or Confirm button
  ↓
GameEvents.CharacterSelected_Event fired
  ↓
RunController.StartNewRun(selectedCharacter)
  ↓
Load DraftScene with selected character
```

## Testing

### Automated Tests
Run the EditMode tests in `Assets/Tests/EditModeTests/CharacterSelectionTests.cs`:
- Database_HasThreeCharacters
- Database_GetByIndex_ReturnsCorrectCharacter
- Controller_Next_WrapsAround
- Controller_Previous_WrapsAround
- Controller_Confirm_FiresCharacterSelectedEvent

### Manual Testing
1. Start the game from MainMenu scene
2. Click "New Game" button
3. Verify CharacterSelectScene loads
4. Test keyboard navigation (← → keys)
5. Verify character name and stats update
6. Test mouse clicks on Previous/Next buttons
7. Press Enter or click Confirm
8. Verify DraftScene loads with selected character stats

## Diagnostic Logging
All key actions log to the console with consistent prefixes:
- `[MainMenu]` - Main menu actions
- `[CharacterSelect]` - Character selection actions
- `[Run]` - Run controller actions

## Architecture Notes
- Character logic is decoupled from MonoBehaviours
- CharacterDefinition is data-driven via ScriptableObjects
- Event system allows for loose coupling between systems
- All tests are EditMode tests (no PlayMode required)
- Selection animation is simple scale punch for determinism

## Placeholder Art
Placeholder images are located in `Assets/Art/Characters/`:
- bob_placeholder.png
- garry_placeholder.png
- steve_placeholder.png

Replace these with final character art when available.
