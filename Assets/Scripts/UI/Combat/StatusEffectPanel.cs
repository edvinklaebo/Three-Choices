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

            RefreshDisplay();
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

            var effects = _unit.StatusEffects;
            var visibleCount = Mathf.Min(effects.Count, _maxVisibleIcons);
            var overflow = effects.Count - visibleCount;
            var iconCount = visibleCount + (overflow > 0 ? 1 : 0);
            ResizeIcons(iconCount);

            for (var i = 0; i < visibleCount; i++)
            {
                var effect = effects[i];
                var icon = _activeIcons[i];
                if (!icon.MatchesEffect(effect.Id, effect.Stacks, effect.Duration))
                {
                    var definition = _library != null ? _library.GetDefinition(effect.Id) : null;
                    icon.SetEffect(effect.Id, effect.Stacks, effect.Duration, definition);
                }
            }

            if (overflow > 0)
                _activeIcons[visibleCount].SetOverflow(overflow);
        }

        private void ResizeIcons(int count)
        {
            while (_activeIcons.Count > count)
            {
                var lastIndex = _activeIcons.Count - 1;
                ReturnIconToPool(_activeIcons[lastIndex]);
                _activeIcons.RemoveAt(lastIndex);
            }

            while (_activeIcons.Count < count)
            {
                var icon = GetIconFromPool();
                icon.gameObject.SetActive(true);
                _activeIcons.Add(icon);
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