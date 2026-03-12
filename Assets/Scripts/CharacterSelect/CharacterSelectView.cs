using System.Collections.Generic;

using Characters;

using TMPro;

using UI;
using UI.Stats;

using UnityEngine;
using UnityEngine.UI;

using Utils;

namespace CharacterSelect
{
    public class CharacterSelectView : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private Image _portraitImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private StatsPanelUI _statsPanel;
        [SerializeField] private Transform _portraitTransform;

        [Header("Configuration")]
        [SerializeField] private float _animationTime = 0.2f;
        [SerializeField] private float _scaleAmount = 0.1f;
    
        private readonly List<StatViewData> _statsBuffer = new(4);
        private Vector3 _originalScale;
        private Coroutine _scaleRoutine;

        private void Awake()
        {
            if (this._portraitTransform == null)
            {
                Log.Error($"{nameof(CharacterSelectView)} missing portrait transform.", this);
                enabled = false;
                return;
            }

            this._originalScale = this._portraitTransform.localScale;
        }

        private void OnDisable()
        {
            if (this._scaleRoutine != null)
            {
                StopCoroutine(this._scaleRoutine);
                this._scaleRoutine = null;
            }

            if (this._portraitTransform)
                this._portraitTransform.localScale = this._originalScale;
        }

        public void DisplayCharacter(CharacterDefinition character)
        {
            if (!character)
                return;

            if (this._portraitImage)
                this._portraitImage.sprite = character.Portrait;

            if (this._nameText)
                this._nameText.text = character.DisplayName;

            if (this._statsPanel)
            {
                this._statsBuffer.Clear();
                this._statsBuffer.Add(new StatViewData("HP", character.MaxHp));
                this._statsBuffer.Add(new StatViewData("Attack", character.Attack));
                this._statsBuffer.Add(new StatViewData("Armor", character.Armor));
                this._statsBuffer.Add(new StatViewData("Speed", character.Speed));
                this._statsPanel.Show(this._statsBuffer);
            }

            PlaySelectionAnimation();
        }

        private void PlaySelectionAnimation()
        {
            if (!this._portraitTransform)
                return;

            if (this._scaleRoutine != null)
                StopCoroutine(this._scaleRoutine);

            var from = this._originalScale + Vector3.one * this._scaleAmount;
            this._scaleRoutine = UIAnimator.AnimateScale(this._portraitTransform, from, this._originalScale, this._animationTime, this);
        }
    }
}