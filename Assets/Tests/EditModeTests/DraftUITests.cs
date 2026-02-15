using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Tests.EditModeTests
{
    public class DraftUITests
    {
        private GameObject _draftUIObject;
        private DraftUI _draftUI;

        [SetUp]
        public void SetUp()
        {
            // Create a GameObject with DraftUI component
            _draftUIObject = new GameObject("TestDraftUI");
            _draftUI = _draftUIObject.AddComponent<DraftUI>();

            // Create mock draft buttons
            _draftUI.DraftButtons = new Button[3];
            for (int i = 0; i < 3; i++)
            {
                var btnObj = new GameObject($"Button{i}");
                btnObj.transform.SetParent(_draftUIObject.transform);
                var btn = btnObj.AddComponent<Button>();
                
                // Add required Text component
                var textObj = new GameObject("Text");
                textObj.transform.SetParent(btnObj.transform);
                textObj.AddComponent<Text>();
                
                // Add required TooltipTrigger component
                btnObj.AddComponent<TooltipTrigger>();
                
                _draftUI.DraftButtons[i] = btn;
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (_draftUIObject != null)
                Object.DestroyImmediate(_draftUIObject);
        }

        [Test]
        public void Awake_CreatesCanvasGroup()
        {
            // Awake is called automatically when component is added
            var canvasGroup = _draftUIObject.GetComponent<CanvasGroup>();
            
            Assert.IsNotNull(canvasGroup, "CanvasGroup should be created in Awake");
        }

        [Test]
        public void Awake_StartsHidden()
        {
            // After Awake, the UI should be hidden
            var canvasGroup = _draftUIObject.GetComponent<CanvasGroup>();
            
            Assert.AreEqual(0f, canvasGroup.alpha, "Alpha should be 0 after Awake");
            Assert.IsFalse(canvasGroup.interactable, "Should not be interactable after Awake");
            Assert.IsFalse(canvasGroup.blocksRaycasts, "Should not block raycasts after Awake");
        }

        [Test]
        public void Hide_WithoutAnimation_SetsCanvasGroupToHidden()
        {
            // First show the UI
            var canvasGroup = _draftUIObject.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            // Hide without animation
            _draftUI.Hide(animated: false);

            Assert.AreEqual(0f, canvasGroup.alpha, "Alpha should be 0 after Hide");
            Assert.IsFalse(canvasGroup.interactable, "Should not be interactable after Hide");
            Assert.IsFalse(canvasGroup.blocksRaycasts, "Should not block raycasts after Hide");
        }

        [Test]
        public void Show_WithoutAnimation_SetsCanvasGroupToVisible()
        {
            // Create a mock draft
            var draft = new List<UpgradeDefinition>
            {
                ScriptableObject.CreateInstance<UpgradeDefinition>(),
                ScriptableObject.CreateInstance<UpgradeDefinition>(),
                ScriptableObject.CreateInstance<UpgradeDefinition>()
            };

            // Set display names to avoid warnings
            for (int i = 0; i < draft.Count; i++)
            {
                draft[i].DisplayName = $"Upgrade {i}";
                draft[i].Description = $"Description {i}";
            }

            // Show without animation
            bool pickCalled = false;
            _draftUI.Show(draft, _ => pickCalled = true, animated: false);

            var canvasGroup = _draftUIObject.GetComponent<CanvasGroup>();
            Assert.AreEqual(1f, canvasGroup.alpha, "Alpha should be 1 after Show");
            Assert.IsTrue(canvasGroup.interactable, "Should be interactable after Show");
            Assert.IsTrue(canvasGroup.blocksRaycasts, "Should block raycasts after Show");
            Assert.IsTrue(_draftUIObject.activeSelf, "GameObject should be active after Show");

            // Cleanup
            foreach (var upgrade in draft)
                Object.DestroyImmediate(upgrade);
        }

        [Test]
        public void Show_ConfiguresButtonsCorrectly()
        {
            var draft = new List<UpgradeDefinition>
            {
                ScriptableObject.CreateInstance<UpgradeDefinition>(),
                ScriptableObject.CreateInstance<UpgradeDefinition>()
            };

            // Set display names
            draft[0].DisplayName = "Upgrade A";
            draft[0].Description = "Description A";
            draft[1].DisplayName = "Upgrade B";
            draft[1].Description = "Description B";

            // Show UI
            _draftUI.Show(draft, _ => { }, animated: false);

            // Check that first 2 buttons are active and third is inactive
            Assert.IsTrue(_draftUI.DraftButtons[0].gameObject.activeSelf, "Button 0 should be active");
            Assert.IsTrue(_draftUI.DraftButtons[1].gameObject.activeSelf, "Button 1 should be active");
            Assert.IsFalse(_draftUI.DraftButtons[2].gameObject.activeSelf, "Button 2 should be inactive");

            // Check button text
            var text0 = _draftUI.DraftButtons[0].GetComponentInChildren<Text>();
            var text1 = _draftUI.DraftButtons[1].GetComponentInChildren<Text>();
            Assert.AreEqual("Upgrade A", text0.text, "Button 0 text should match upgrade name");
            Assert.AreEqual("Upgrade B", text1.text, "Button 1 text should match upgrade name");

            // Cleanup
            foreach (var upgrade in draft)
                Object.DestroyImmediate(upgrade);
        }

        [Test]
        public void Show_WithNullDraft_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _draftUI.Show(null, _ => { }, animated: false));
        }

        [Test]
        public void Show_WithEmptyDraft_DoesNotThrow()
        {
            var emptyDraft = new List<UpgradeDefinition>();
            Assert.DoesNotThrow(() => _draftUI.Show(emptyDraft, _ => { }, animated: false));
        }
    }
}
