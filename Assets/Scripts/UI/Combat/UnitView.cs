using Core;

using UnityEngine;

using Utils;

namespace UI.Combat
{
    /// <summary>
    /// Visual representation of a combat unit (player or enemy).
    /// Handles positioning, facing direction, and animation orchestration.
    /// Purely visual - no game logic.
    /// </summary>
    public class UnitView : MonoBehaviour
    {
        [Header("Position Points")]
        [SerializeField] private Transform _idlePoint;
        [SerializeField] private Transform _lungePoint;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
    
        private Unit _unit;
        private bool _isPlayer;
        private Vector3 _spriteOriginalLocalPosition;

        public Transform IdlePoint => this._idlePoint;
        public Transform LungePoint => this._lungePoint;
        public Unit Unit => this._unit;
        public bool IsPlayer => this._isPlayer;
        public Transform SpriteTransform => this._spriteRenderer?.transform;

        private void Awake()
        {
            ValidateComponents();
        }

        /// <summary>
        /// Initialize the unit view with a unit reference.
        /// </summary>
        public void Initialize(Unit unit, bool isPlayer, Sprite portrait)
        {
            if (unit == null)
            {
                Log.Error("UnitView: Cannot initialize with null unit");
                return;
            }

            this._unit = unit;
            this._isPlayer = isPlayer;

            // Position at idle point
            if (this._idlePoint != null)
            {
                transform.position = this._idlePoint.position;
            }

            // Store sprite's original local position
            if (this._spriteRenderer != null)
            {
                this._spriteOriginalLocalPosition = this._spriteRenderer.transform.localPosition;
            }

            if (portrait != null)
            {
                SetSprite(portrait);
            }

            // Set facing direction
            SetFacingDirection(isPlayer);

            Log.Info("UnitView initialized", new
            {
                unit = unit.Name,
                isPlayer
            });
        }

        /// <summary>
        /// Set the sprite for this unit.
        /// </summary>
        public void SetSprite(Sprite sprite)
        {
            if (this._spriteRenderer != null)
            {
                this._spriteRenderer.sprite = sprite;
            }
        }

        /// <summary>
        /// Reset sprite to its original local position (e.g., after lunge animation).
        /// </summary>
        public void ResetSpritePosition()
        {
            if (this._spriteRenderer != null)
            {
                this._spriteRenderer.transform.localPosition = this._spriteOriginalLocalPosition;
            }
        }

        /// <summary>
        /// Set facing direction based on role.
        /// Player faces right, enemy faces left.
        /// </summary>
        private void SetFacingDirection(bool isPlayer)
        {
            // Ensure sprite renderer is found if not already assigned
            if (this._spriteRenderer == null)
            {
                this._spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (this._spriteRenderer != null)
            {
                // Player faces right (positive X), enemy faces left (negative X)
                this._spriteRenderer.flipX = !isPlayer;
            }
        }

        private void ValidateComponents()
        {
            if (this._idlePoint == null)
            {
                Log.Warning($"UnitView [{gameObject.name}]: IdlePoint not assigned");
            }

            if (this._lungePoint == null)
            {
                Log.Warning($"UnitView [{gameObject.name}]: LungePoint not assigned");
            }

            if (this._spriteRenderer == null)
            {
                this._spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (this._spriteRenderer == null)
                {
                    Log.Warning($"UnitView [{gameObject.name}]: No SpriteRenderer found");
                }
            }
        }
    }
}
