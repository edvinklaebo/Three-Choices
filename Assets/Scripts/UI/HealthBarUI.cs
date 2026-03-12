using System;
using System.Collections;

using Core;

using UnityEngine;
using UnityEngine.UI;

using Utils;

namespace UI
{
    /// <summary>
    ///     Reusable health bar UI component that displays a Unit's current HP as a normalized slider.
    ///     Features:
    ///     - Displays health as a normalized value (0-1) using Unity's Slider component
    ///     - Smooth lerp animation for health transitions
    ///     - Works with any Unit (player, enemy, etc.)
    ///     Usage:
    ///     1. Attach to a GameObject with a Slider component
    ///     2. Assign the Slider in the inspector (or it will auto-find on the same GameObject)
    ///     3. Call Bind(unit) with the Unit to track
    ///     4. Health bar animates only when AnimateToHealth() is called
    ///     5. Call Unbind() when done tracking the unit
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private float _animationDuration = 0.25f;

        private Unit _unit;
        private Coroutine _animation;

        public void Awake()
        {
            if (this._slider == null)
                this._slider = GetComponent<Slider>();

            if (this._slider == null)
                throw new InvalidOperationException("HealthBarUI requires a Slider.");
        
            this._slider.interactable = false;
        }

        /// <summary>
        ///     Binds this health bar to a unit. Stops any active animation and immediately
        ///     resets the slider to the unit's current normalized HP.
        /// </summary>
        public void Bind(Unit unit)
        {
            if (unit == null)
            {
                Log.Error("HealthBarUI: Cannot bind null unit");
                return;
            }

            StopActiveAnimation();
            this._unit = unit;

            var maxHP = this._unit.Stats.MaxHP;
            this._slider.value = NormalizeHP(this._unit.Stats.CurrentHP, maxHP);
        }

        /// <summary>
        ///     Immediately sets the slider to the specified HP value without animation.
        ///     Use this to initialize the display to a specific HP value (e.g. pre-combat state).
        /// </summary>
        public void SnapToHealth(int currentHP, int maxHP)
        {
            if (!this._slider)
                return;

            StopActiveAnimation();
            this._slider.value = NormalizeHP(currentHP, maxHP);
        }

        /// <summary>
        ///     Unbinds the current unit. Stops any active animation and clears the unit reference.
        /// </summary>
        public void Unbind()
        {
            StopActiveAnimation();
            this._unit = null;
        }

        /// <summary>
        ///     Animates the health bar from a specific HP value to another HP value.
        ///     This allows proper animation even when the units state has already changed.
        ///     Used when combat logic pre-calculates all state changes before presentation.
        /// </summary>
        /// <param name="hpBefore">Starting HP value</param>
        /// <param name="hpAfter">Target HP value</param>
        public void AnimateToHealth(int hpBefore, int hpAfter)
        {
            if (this._unit == null)
            {
                Log.Error("HealthBarUI: AnimateToHealth called with no unit bound");
                return;
            }

            if (!this._slider)
                return;

            var maxHP = this._unit.Stats.MaxHP;
            if (maxHP <= 0)
                return;

            var from = NormalizeHP(hpBefore, maxHP);
            var to = NormalizeHP(hpAfter, maxHP);

            StopActiveAnimation();

            this._animation = StartCoroutine(AnimateRoutine(from, to));
        }

        private void StopActiveAnimation()
        {
            if (this._animation == null) 
                return;
        
            StopCoroutine(this._animation);
            this._animation = null;
        }

        private static float NormalizeHP(int hp, int maxHP) =>
            maxHP > 0 ? Mathf.Clamp01((float)hp / maxHP) : 0f;

        private IEnumerator AnimateRoutine(float from, float to)
        {
            var elapsed = 0f;

            this._slider.value = from;

            while (elapsed < this._animationDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / this._animationDuration;
                this._slider.value = Mathf.Lerp(from, to, t);
                yield return null;
            }

            this._slider.value = to;
        }
    }
}