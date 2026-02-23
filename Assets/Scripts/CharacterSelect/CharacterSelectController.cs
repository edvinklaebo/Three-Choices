using System;
using UnityEngine;

public class CharacterSelectController : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private CharacterCollection _collection;
    [SerializeField] private CharacterSelectView _view;

    public int CurrentIndex { get; private set; }

    private CharacterDefinition _current => _collection?.GetByIndex(CurrentIndex);


    private void Awake()
    {
        if (_collection == null)
            throw new InvalidOperationException(
                $"CharacterSelectController requires a {nameof(CharacterCollection)} assigned in the inspector.");
        if (_view == null)
            throw new InvalidOperationException(
                $"CharacterSelectController requires a {nameof(CharacterSelectView)} assigned in the inspector.");
    }

    private void OnEnable()
    {
        UpdateView();
    }

    public void Next()
    {
        if (_collection.Characters == null || _collection.Characters.Count == 0)
            return;

        CurrentIndex = (CurrentIndex + 1) % _collection.Characters.Count;
        Log.Info($"[CharacterSelect] Next → {_current.Id}");
        UpdateView();
    }

    public void Previous()
    {
        if (_collection.Characters == null || _collection.Characters.Count == 0)
            return;

        CurrentIndex = (CurrentIndex - 1 + _collection.Characters.Count) % _collection.Characters.Count;
        Log.Info($"[CharacterSelect] Prev → {_current.Id}");
        UpdateView();
    }

    public void Confirm()
    {
        if (!_current)
        {
            Log.Error("[CharacterSelect] Cannot confirm - no character selected");
            return;
        }

        Log.Info($"[CharacterSelect] Confirmed {_current.Id}");
        GameEvents.CharacterSelected_Event?.Invoke(_current);
    }

    private void UpdateView()
    {
        if (_view && _current)
            _view.DisplayCharacter(_current);
    }
}