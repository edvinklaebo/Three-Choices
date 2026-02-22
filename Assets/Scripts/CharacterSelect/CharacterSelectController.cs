using UnityEngine;
using UnityEngine.Serialization;

public class CharacterSelectController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterCollection _collection;
    [SerializeField] private CharacterSelectView _view;

    public int CurrentIndex { get; private set; }

    private CharacterDefinition _current => _collection?.GetByIndex(CurrentIndex);

    private void OnEnable()
    {
        UpdateView();
    }

    public void Next()
    {
        if (_collection == null || _collection.Characters == null || _collection.Characters.Count == 0)
            return;

        CurrentIndex = (CurrentIndex + 1) % _collection.Characters.Count;
        Log.Info($"[CharacterSelect] Next → {_current.Id}");
        UpdateView();
    }

    public void Previous()
    {
        if (_collection == null || _collection.Characters == null || _collection.Characters.Count == 0)
            return;

        CurrentIndex = (CurrentIndex - 1 + _collection.Characters.Count) % _collection.Characters.Count;
        Log.Info($"[CharacterSelect] Prev → {_current.Id}");
        UpdateView();
    }

    public void Confirm()
    {
        if (_current == null)
        {
            Log.Error("[CharacterSelect] Cannot confirm - no character selected");
            return;
        }

        Log.Info($"[CharacterSelect] Confirmed {_current.Id}");
        GameEvents.CharacterSelected_Event?.Invoke(_current);
    }

    private void UpdateView()
    {
        if (_view != null && _current != null) _view.DisplayCharacter(_current);
    }
}