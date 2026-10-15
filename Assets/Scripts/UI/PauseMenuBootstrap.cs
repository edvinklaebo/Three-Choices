using Core;
using Events;
using Systems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Utils;

namespace UI
{
    /// <summary>
    /// Helper to create pause menu UI programmatically.
    /// This allows the pause system to work even if UI prefabs aren't set up.
    /// For production, create proper prefabs and remove this bootstrapper.
    /// </summary>
    public class PauseMenuBootstrap : MonoBehaviour
    {
        private static PauseMenuBootstrap _instance;

        [SerializeField] private bool _createUIAtStart = true;
        [FormerlySerializedAs("canvas")]
        [SerializeField] private Canvas _canvas = null;
        [SerializeField] private FightStartedEventChannel _fightStarted = null;

        [System.NonSerialized] private Unit _player;

        private void OnEnable()
        {
            if (_fightStarted != null)
                _fightStarted.OnRaised += HandleFightStarted;
        }

        private void OnDisable()
        {
            if (_fightStarted != null)
                _fightStarted.OnRaised -= HandleFightStarted;
        }

        private void HandleFightStarted(Unit player, int fightIndex)
        {
            _player = player;
        }

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        private void Start()
        {
            if (_createUIAtStart)
            {
                CreatePauseMenuUI();
            }
        }

        private void CreatePauseMenuUI()
        {
            if (_canvas == null)
                throw new System.InvalidOperationException($"PauseMenuBootstrap '{name}' requires a canvas.");

            // Create PauseMenuUI root
            var pauseMenuRoot = new GameObject("PauseMenuUI");
            pauseMenuRoot.transform.SetParent(_canvas.transform, false);
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

            CreateButton(menuContent.transform, "Resume", PauseMenuUI.OnResumeClicked);
            CreateButton(menuContent.transform, "Main Menu", PauseMenuUI.OnMainMenuClicked);
            CreateButton(menuContent.transform, "Settings", pauseMenuUI.OnSettingsClicked);
            if (_fightStarted != null)
                CreateButton(menuContent.transform, "Obtained Items", pauseMenuUI.OnObtainedItemsClicked);
            var quitBtn = CreateButton(menuContent.transform, "Quit", PauseMenuUI.OnQuitClicked);

            // Create Settings Panel
            var settingsPanel = CreatePanel(pauseMenuRoot.transform, "SettingsPanel");
            AddDarkBackground(settingsPanel.transform);

            var settingsContent = CreateVerticalLayout(settingsPanel.transform, "SettingsContent");
            AddTitle(settingsContent.transform, "SETTINGS");

            // Add volume slider
            CreateSliderRow(settingsContent.transform, "Master Volume", out var volumeSlider);

            // Add fullscreen toggle
            CreateToggleRow(settingsContent.transform, "Fullscreen", out var fullscreenToggle);

            // Add back button
            CreateButton(settingsContent.transform, "Back", pauseMenuUI.OnBackFromSettings);

            // Add SettingsPanel component
            var settingsPanelComponent = settingsPanel.AddComponent<SettingsPanel>();
            settingsPanelComponent.Initialize(volumeSlider, fullscreenToggle);

            // Wire up PauseMenuUI references
            pauseMenuUI.Initialize(pausePanel, settingsPanel, quitBtn.gameObject);
            if (_fightStarted != null)
            {
                var overview = CreateOverview(pauseMenuRoot.transform, pauseMenuUI.OnBackFromOverview);
                pauseMenuUI.InitializeOverview(overview,
                    () => ObtainedItemsFormatter.Format(_player?.ObtainedItems,
                        _player?.HasCompleteObtainedItemHistory ?? false));
            }

            // Add PauseInput to canvas
            if (_canvas.GetComponent<PauseInput>() == null)
                _canvas.gameObject.AddComponent<PauseInput>();

            Log.Info("PauseMenuUI created successfully");
        }

        private static PlayerOverviewUI CreateOverview(Transform parent, UnityAction onBack)
        {
            var panel = CreatePanel(parent, "ObtainedItemsPanel");
            AddDarkBackground(panel.transform);
            var title = CreatePanel(panel.transform, "Title");
            SetRegion(title, new Vector2(0.1f, 0.88f), new Vector2(0.9f, 0.98f));
            AddTitle(title.transform, "OBTAINED ITEMS");
            var titleRect = (RectTransform)title.transform.GetChild(0);
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.sizeDelta = Vector2.zero;

            var scroll = CreateOverviewScroll(panel.transform);
            var overview = panel.AddComponent<PlayerOverviewUI>();
            overview.Initialize(scroll.content.GetComponent<TMP_Text>(), scroll);
            var back = CreateButton(panel.transform, "Back", onBack);
            SetRegion(back.gameObject, new Vector2(0.4f, 0.02f), new Vector2(0.6f, 0.1f));
            return overview;
        }

        private static ScrollRect CreateOverviewScroll(Transform parent)
        {
            var area = CreatePanel(parent, "Scroll");
            SetRegion(area, new Vector2(0.12f, 0.14f), new Vector2(0.88f, 0.86f));
            var scroll = area.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;

            var viewport = CreatePanel(area.transform, "Viewport");
            viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f);
            viewport.AddComponent<RectMask2D>();
            scroll.viewport = (RectTransform)viewport.transform;
            scroll.content = CreateOverviewContent(viewport.transform);
            return scroll;
        }

        private static RectTransform CreateOverviewContent(Transform parent)
        {
            var content = CreatePanel(parent, "Content");
            var rect = (RectTransform)content.transform;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 1);
            var text = content.AddComponent<TextMeshProUGUI>();
            text.fontSize = 28;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.richText = false;
            text.raycastTarget = false;
            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rect;
        }

        private static void SetRegion(GameObject target, Vector2 min, Vector2 max)
        {
            var rect = (RectTransform)target.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        private static GameObject CreatePanel(Transform parent, string name)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            return panel;
        }

        private static void AddDarkBackground(Transform panel)
        {
            var bg = new GameObject("Background");
            bg.transform.SetParent(panel, false);

            var rect = bg.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var image = bg.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.8f);
        }

        private static GameObject CreateVerticalLayout(Transform parent, string name)
        {
            var layout = new GameObject(name);
            layout.transform.SetParent(parent, false);

            var rect = layout.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(400, 500);

            var vlg = layout.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            return layout;
        }

        private static void AddTitle(Transform parent, string text)
        {
            var title = new GameObject("Title");
            title.transform.SetParent(parent, false);

            var rect = title.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 80);

            var tmp = title.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 48;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }

        private static Button CreateButton(Transform parent, string text, UnityAction onClick)
        {
            var btnObj = new GameObject($"Button_{text}");
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 60);

            var image = btnObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            var button = btnObj.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            // Add text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return button;
        }

        private static void CreateSliderRow(Transform parent, string label, out Slider slider)
        {
            var row = new GameObject($"SliderRow_{label}");
            row.transform.SetParent(parent, false);

            var rect = row.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 60);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childControlWidth = true;
            hlg.childForceExpandWidth = false;

            // Label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);
            var labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 150;

            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = Color.white;

            // Slider
            var sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(row.transform, false);
            var sliderLayout = sliderObj.AddComponent<LayoutElement>();
            sliderLayout.preferredWidth = 200;

            slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(sliderObj.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Fill Area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = new Vector2(-10, -10);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.3f, 0.6f, 1f, 1f);

            // Handle Slide Area
            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            var handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.sizeDelta = new Vector2(-10, 0);

            var handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            var handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
        }

        private static void CreateToggleRow(Transform parent, string label, out Toggle toggle)
        {
            var row = new GameObject($"ToggleRow_{label}");
            row.transform.SetParent(parent, false);

            var rect = row.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 60);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childControlWidth = false;

            // Label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);
            var labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 150;

            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = Color.white;

            // Toggle
            var toggleObj = new GameObject("Toggle");
            toggleObj.transform.SetParent(row.transform, false);

            var toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(60, 40);

            toggle = toggleObj.AddComponent<Toggle>();
            toggle.isOn = true;

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(toggleObj.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            toggle.targetGraphic = bgImage;

            // Checkmark
            var checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(bg.transform, false);
            var checkRect = checkmark.AddComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.sizeDelta = new Vector2(-10, -10);
            var checkImage = checkmark.AddComponent<Image>();
            checkImage.color = new Color(0.3f, 0.6f, 1f, 1f);

            toggle.graphic = checkImage;
        }
    }
}