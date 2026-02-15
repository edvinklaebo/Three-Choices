# Pause Menu Setup Instructions

## Automated Setup (Recommended)

Add the `PauseMenuBootstrap` component to any GameObject in your gameplay scene (DraftScene):

1. Open the DraftScene in Unity Editor
2. Create an empty GameObject (Right-click in Hierarchy > Create Empty)
3. Rename it to "PauseMenuBootstrap"
4. Add the `PauseMenuBootstrap` component to it
5. The pause menu UI will be automatically created at runtime

## Manual Setup (For Production)

For a production-ready setup, create proper UI prefabs instead of using the bootstrap:

### Canvas Setup
1. Create a Canvas (UI > Canvas) if one doesn't exist
2. Set Canvas Scaler to "Scale With Screen Size"
3. Add `PauseInput` component to the Canvas

### Pause Menu Panel
1. Create Panel: `Canvas > PauseMenuPanel`
2. Add semi-transparent black background
3. Add vertical layout group with these children:
   - Title Text: "PAUSED"
   - Resume Button
   - Settings Button
   - Quit Button (goes to Main Menu)

### Settings Panel
1. Create Panel: `Canvas > SettingsPanel`
2. Add semi-transparent black background
3. Add vertical layout group with these children:
   - Title Text: "SETTINGS"
   - Volume Slider Row (label + slider)
   - Fullscreen Toggle Row (label + toggle)
   - Back Button

### Wire Up Components
1. Add `PauseMenuUI` component to a root GameObject
2. Assign references:
   - `_pauseMenuPanel` → PauseMenuPanel GameObject
   - `_settingsPanel` → SettingsPanel GameObject

3. Add `SettingsPanel` component to the SettingsPanel GameObject
4. Assign references:
   - `_volumeSlider` → Volume Slider
   - `_fullscreenToggle` → Fullscreen Toggle

### Button Callbacks
Wire up button onClick events in the Inspector:
- Resume Button → `PauseMenuUI.OnResumeClicked`
- Settings Button → `PauseMenuUI.OnSettingsClicked`
- Back Button → `PauseMenuUI.OnBackFromSettings`
- Quit Button → `PauseMenuUI.OnQuitClicked`

## Testing

1. Play the scene
2. Press `Esc` to open pause menu
3. Game should freeze (Time.timeScale = 0)
4. Test all buttons
5. Test volume slider
6. Test fullscreen toggle
7. Press `Esc` again to resume

## Notes

- Both pause menu panel and settings panel should be **disabled by default** in the hierarchy
- The system will automatically show/hide them based on pause state
- The pause system works in any scene where you add the components
