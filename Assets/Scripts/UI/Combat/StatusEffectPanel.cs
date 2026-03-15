using System.Collections.Generic;

using Core;
using Core.StatusEffects;

using UnityEngine;

using Utils;

namespace UI.Combat
{
    /// <summary>
    ///     Status effects panel for a unit.
    ///     Displays icons for active status effects with stack counts and hover tooltips.
    ///     Requires a <see cref="StatusEffectLibrary" /> to resolve definition metadata (icon, description).
    ///     Updates automatically when the unit's status effects change.
    ///     Handles overflow with "+N" indicator.
    /// </summary>
    public class StatusEffectPanel : MonoBehaviour
    {
        [SerializeField] private StatusEffectIcon _iconPrefab;
        [SerializeField] private Transform _iconContainer;
        [SerializeField] private StatusEffectLibrary _library;
        [SerializeField] private int _maxVisibleIcons = 6;

        private readonly List<StatusEffectIcon> _activeIcons = new();
        private readonly Stack<StatusEffectIcon> _iconPool = new();
        private int _lastEffectCount = -1;

        private Unit _unit;

        private void Awake()
        {
            if (_iconPrefab == null) Log.Error("StatusEffectPanel: IconPrefab not assigned");
            if (_library == null) Log.Warning("StatusEffectPanel: StatusEffectLibrary not assigned — icons will use fallback colors");

            if (_iconContainer == null) _iconContainer = transform;
        }

        private void Update()
        {
            if (_unit == null || _unit.StatusEffects == null)
                return;

            if (_unit.StatusEffects.Count != _lastEffectCount)
            {
                RefreshDisplay();
                _lastEffectCount = _unit.StatusEffects.Count;
            }
        }

        /// <summary>
        ///     Initialize the panel for the given <paramref name="unit" />.
        /// </summary>
        public void Initialize(Unit unit)
        {
            if (unit == null)
            {
                Log.Error("StatusEffectPanel: Cannot initialize with null unit");
                return;
            }

            _unit = unit;
            RefreshDisplay();

            Log.Info("StatusEffectPanel initialized", new { unit = unit.Name });
        }

        /// <summary>
        ///     Refresh the status effect display based on the unit's current effects.
        /// </summary>
        private void RefreshDisplay()
        {
            if (_unit == null || _unit.StatusEffects == null)
                return;

            foreach (var icon in _activeIcons) ReturnIconToPool(icon);
            _activeIcons.Clear();

            var effects = _unit.StatusEffects;
            var visibleCount = Mathf.Min(effects.Count, _maxVisibleIcons);

            for (var i = 0; i < visibleCount; i++)
            {
                var effect = effects[i];
                var definition = _library != null ? _library.GetDefinition(effect.Id) : null;
                var icon = GetIconFromPool();

                icon.SetEffect(effect.Id, effect.Stacks, effect.Duration, definition);
                icon.gameObject.SetActive(true);

                _activeIcons.Add(icon);
            }

            var overflow = effects.Count - visibleCount;
            if (overflow > 0)
            {
                var overflowIcon = GetIconFromPool();
                overflowIcon.SetOverflow(overflow);
                overflowIcon.gameObject.SetActive(true);
                _activeIcons.Add(overflowIcon);
            }
        }

        private StatusEffectIcon GetIconFromPool()
        {
            if (_iconPool.Count > 0) return _iconPool.Pop();

            return Instantiate(_iconPrefab, _iconContainer);
        }

        private void ReturnIconToPool(StatusEffectIcon icon)
        {
            if (icon == null)
                return;

            icon.gameObject.SetActive(false);
            _iconPool.Push(icon);
        }
    }
}