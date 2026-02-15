using UnityEngine;

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

    public Transform IdlePoint => _idlePoint;
    public Transform LungePoint => _lungePoint;
    public Unit Unit => _unit;
    public bool IsPlayer => _isPlayer;
    public Transform SpriteTransform => _spriteRenderer?.transform;

    private void Awake()
    {
        ValidateComponents();
    }

    /// <summary>
    /// Initialize the unit view with a unit reference.
    /// </summary>
    public void Initialize(Unit unit, bool isPlayer)
    {
        if (unit == null)
        {
            Debug.LogError("UnitView: Cannot initialize with null unit");
            return;
        }

        _unit = unit;
        _isPlayer = isPlayer;

        // Position at idle point
        if (_idlePoint != null)
        {
            transform.position = _idlePoint.position;
        }

        // Store sprite's original local position
        if (_spriteRenderer != null)
        {
            _spriteOriginalLocalPosition = _spriteRenderer.transform.localPosition;
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
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = sprite;
        }
    }

    /// <summary>
    /// Reset sprite to its original local position (e.g., after lunge animation).
    /// </summary>
    public void ResetSpritePosition()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.transform.localPosition = _spriteOriginalLocalPosition;
        }
    }

    /// <summary>
    /// Set facing direction based on role.
    /// Player faces right, enemy faces left.
    /// </summary>
    private void SetFacingDirection(bool isPlayer)
    {
        // Ensure sprite renderer is found if not already assigned
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (_spriteRenderer != null)
        {
            // Player faces right (positive X), enemy faces left (negative X)
            _spriteRenderer.flipX = !isPlayer;
        }
    }

    private void ValidateComponents()
    {
        if (_idlePoint == null)
        {
            Debug.LogWarning($"UnitView [{gameObject.name}]: IdlePoint not assigned");
        }

        if (_lungePoint == null)
        {
            Debug.LogWarning($"UnitView [{gameObject.name}]: LungePoint not assigned");
        }

        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                Debug.LogWarning($"UnitView [{gameObject.name}]: No SpriteRenderer found");
            }
        }
    }
}
