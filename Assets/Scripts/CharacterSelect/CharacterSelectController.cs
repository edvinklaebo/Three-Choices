using System;
using UnityEngine;

public class CharacterSelectController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterCollection _collection;
    [SerializeField] private CharacterSelectView _view;

    private CharacterSelectionModel _model;

    public int CurrentIndex => _model?.CurrentIndex ?? 0;

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
        UpdateView();
    }

    public void Next()
    {
        if (_model == null)
        {
            Log.Warning("[CharacterSelect] Next called before model was initialized.");
            return;
        }

        _model.Next();
        Log.Info($"[CharacterSelect] Next → {_model.Current.Id}");
        UpdateView();
    }

    public void Previous()
    {
        if (_model == null)
        {
            Log.Warning("[CharacterSelect] Previous called before model was initialized.");
            return;
        }

        _model.Previous();
        Log.Info($"[CharacterSelect] Prev → {_model.Current.Id}");
        UpdateView();
    }

    public void Confirm()
    {
        if (_model == null || !_model.Current)
        {
            Log.Error("[CharacterSelect] Cannot confirm - no character selected");
            return;
        }

        Log.Info($"[CharacterSelect] Confirmed {_model.Current.Id}");
        GameEvents.CharacterSelected_Event?.Invoke(_model.Current);
    }

    private void UpdateView()
    {
        if (_view && _model != null && _model.Current)
            _view.DisplayCharacter(_model.Current);
    }
}