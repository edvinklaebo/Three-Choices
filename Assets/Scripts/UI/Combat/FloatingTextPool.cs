using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Object pool for floating text instances.
///     Manages creation and reuse of damage number UI elements.
/// </summary>
public class FloatingTextPool : MonoBehaviour
{
    [SerializeField] private FloatingText _floatingTextPrefab;
    [SerializeField] private Transform _container;
    [SerializeField] private int _initialPoolSize = 10;
    [SerializeField] private Camera _camera;

    private readonly Stack<FloatingText> _pool = new();

    public static FloatingTextPool Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (_container == null) _container = transform;

        if (_camera == null) _camera = Camera.main;

        // Pre-populate pool
        for (var i = 0; i < _initialPoolSize; i++) CreateNewInstance();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    ///     Spawn a floating text at world position.
    /// </summary>
    public void Spawn(int amount, DamageType damageType, Vector3 worldPosition)
    {
        var text = Get();
        if (text != null) text.Show(amount, damageType, worldPosition, _camera);
    }

    /// <summary>
    ///     Get a floating text instance from pool.
    /// </summary>
    private FloatingText Get()
    {
        FloatingText text;

        if (_pool.Count > 0)
            text = _pool.Pop();
        else
            text = CreateNewInstance();

        text.gameObject.SetActive(true);
        text.Reset();

        return text;
    }

    /// <summary>
    ///     Return a floating text instance to pool.
    /// </summary>
    public void Return(FloatingText text)
    {
        if (text == null)
            return;

        text.gameObject.SetActive(false);
        _pool.Push(text);
    }

    /// <summary>
    ///     Create a new floating text instance.
    /// </summary>
    private FloatingText CreateNewInstance()
    {
        if (_floatingTextPrefab == null)
        {
            Log.Error("FloatingTextPool: Prefab not assigned");
            return null;
        }

        var instance = Instantiate(_floatingTextPrefab, _container);
        instance.gameObject.SetActive(false);
        _pool.Push(instance);

        return instance;
    }
}