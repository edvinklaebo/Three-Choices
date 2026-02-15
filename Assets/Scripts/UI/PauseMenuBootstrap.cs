using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Helper to create pause menu UI programmatically.
/// This allows the pause system to work even if UI prefabs aren't set up.
/// For production, create proper prefabs and remove this bootstrapper.
/// </summary>
public class PauseMenuBootstrap : MonoBehaviour
{
    [SerializeField] private bool _createUIAtStart = true;

    private void Start()
    {
        if (_createUIAtStart)
        {
            CreatePauseMenuUI();
        }
    }

    private void CreatePauseMenuUI()
    {
        // Find or create Canvas
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create PauseMenuUI root
        var pauseMenuRoot = new GameObject("PauseMenuUI");
        pauseMenuRoot.transform.SetParent(canvas.transform, false);
        var rootRect = pauseMenuRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;

        // Add PauseMenuUI component
        var pauseMenuUI = pauseMenuRoot.AddComponent<PauseMenuUI>();

        // Create Pause Menu Panel
        var pausePanel = CreatePanel(pauseMenuRoot.transform, "PauseMenuPanel");
        AddDarkBackground(pausePanel.transform);
        
        var menuContent = CreateVerticalLayout(pausePanel.transform, "MenuContent");
        AddTitle(menuContent.transform, "PAUSED");
        
        var resumeBtn = CreateButton(menuContent.transform, "Resume", pauseMenuUI.OnResumeClicked);
        var settingsBtn = CreateButton(menuContent.transform, "Settings", pauseMenuUI.OnSettingsClicked);
        var restartBtn = CreateButton(menuContent.transform, "Restart", pauseMenuUI.OnRestartClicked);
        var quitBtn = CreateButton(menuContent.transform, "Main Menu", pauseMenuUI.OnQuitClicked);

        // Create Settings Panel
        var settingsPanel = CreatePanel(pauseMenuRoot.transform, "SettingsPanel");
        AddDarkBackground(settingsPanel.transform);
        
        var settingsContent = CreateVerticalLayout(settingsPanel.transform, "SettingsContent");
        AddTitle(settingsContent.transform, "SETTINGS");

        // Add volume slider
        var volumeRow = CreateSliderRow(settingsContent.transform, "Master Volume", out var volumeSlider);
        
        // Add fullscreen toggle
        var fullscreenRow = CreateToggleRow(settingsContent.transform, "Fullscreen", out var fullscreenToggle);
        
        // Add back button
        var backBtn = CreateButton(settingsContent.transform, "Back", pauseMenuUI.OnBackFromSettings);

        // Add SettingsPanel component
        var settingsPanelComponent = settingsPanel.AddComponent<SettingsPanel>();
        settingsPanelComponent.Initialize(volumeSlider, fullscreenToggle);

        // Wire up PauseMenuUI references
        pauseMenuUI.Initialize(pausePanel, settingsPanel);

        // Add PauseInput to canvas
        canvas.gameObject.AddComponent<PauseInput>();

        Debug.Log("PauseMenuUI created successfully");
    }

    private GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        
        return panel;
    }

    private void AddDarkBackground(Transform panel)
    {
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(panel, false);
        
        RectTransform rect = bg.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        
        Image image = bg.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.8f);
    }

    private GameObject CreateVerticalLayout(Transform parent, string name)
    {
        GameObject layout = new GameObject(name);
        layout.transform.SetParent(parent, false);
        
        RectTransform rect = layout.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(400, 500);
        
        VerticalLayoutGroup vlg = layout.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.padding = new RectOffset(20, 20, 20, 20);
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        
        return layout;
    }

    private void AddTitle(Transform parent, string text)
    {
        GameObject title = new GameObject("Title");
        title.transform.SetParent(parent, false);
        
        RectTransform rect = title.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 80);
        
        TextMeshProUGUI tmp = title.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 48;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    private Button CreateButton(Transform parent, string text, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject($"Button_{text}");
        btnObj.transform.SetParent(parent, false);
        
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 60);
        
        Image image = btnObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        Button button = btnObj.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        return button;
    }

    private GameObject CreateSliderRow(Transform parent, string label, out Slider slider)
    {
        GameObject row = new GameObject($"SliderRow_{label}");
        row.transform.SetParent(parent, false);
        
        RectTransform rect = row.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 60);
        
        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childControlWidth = true;
        hlg.childForceExpandWidth = false;
        
        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(row.transform, false);
        LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 150;
        
        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;
        
        // Slider
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(row.transform, false);
        LayoutElement sliderLayout = sliderObj.AddComponent<LayoutElement>();
        sliderLayout.preferredWidth = 300;
        sliderLayout.preferredHeight = 20;
        
        slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-10, -10);
        
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.6f, 1f, 1f);
        
        // Handle Slide Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = new Vector2(-10, 0);
        
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;
        
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        
        return row;
    }

    private GameObject CreateToggleRow(Transform parent, string label, out Toggle toggle)
    {
        GameObject row = new GameObject($"ToggleRow_{label}");
        row.transform.SetParent(parent, false);
        
        RectTransform rect = row.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 60);
        
        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childControlWidth = false;
        
        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(row.transform, false);
        LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 150;
        
        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;
        
        // Toggle
        GameObject toggleObj = new GameObject("Toggle");
        toggleObj.transform.SetParent(row.transform, false);
        
        RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
        toggleRect.sizeDelta = new Vector2(60, 40);
        
        toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = true;
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(toggleObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        toggle.targetGraphic = bgImage;
        
        // Checkmark
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(bg.transform, false);
        RectTransform checkRect = checkmark.AddComponent<RectTransform>();
        checkRect.anchorMin = Vector2.zero;
        checkRect.anchorMax = Vector2.one;
        checkRect.sizeDelta = new Vector2(-10, -10);
        Image checkImage = checkmark.AddComponent<Image>();
        checkImage.color = new Color(0.3f, 0.6f, 1f, 1f);
        
        toggle.graphic = checkImage;
        
        return row;
    }
}
