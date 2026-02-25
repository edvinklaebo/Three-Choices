using System;
using UnityEngine;

public class CharacterSelectController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterCollection _collection;
    [SerializeField] private CharacterSelectView _view;

    private CharacterSelectionModel _model;

    public int CurrentIndex => _model.CurrentIndex;
    
    private void Awake()
    {
        if (_collection == null)
            throw new InvalidOperationException(
                $"CharacterSelectController requires a {nameof(CharacterCollection)} assigned in the inspector.");
        if (_view == null)
            throw new InvalidOperationException(
                $"CharacterSelectController requires a {nameof(CharacterSelectView)} assigned in the inspector.");

        _model = new CharacterSelectionModel(_collection.Characters);
    }

    private void OnEnable()
    {
        _view.DisplayCharacter(_model.Current);
    }

    public void Next()
    {
        _model.Next();
        _view.DisplayCharacter(_model.Current);
    }

    public void Previous()
    {
        _model.Previous();
        _view.DisplayCharacter(_model.Current);
    }

    public void Confirm()
    {
        GameEvents.CharacterSelected_Event?.Invoke(_model.Current);
    }
}