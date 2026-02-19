using UnityEngine;

public class CharacterSelectController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterDatabase _database;
    [SerializeField] private CharacterSelectView _view;

    public int CurrentIndex { get; private set; }

    private CharacterDefinition _current => _database?.GetByIndex(CurrentIndex);

    private void OnEnable()
    {
        UpdateView();
    }

    public void Next()
    {
        if (_database == null || _database.Characters == null || _database.Characters.Count == 0)
            return;

        CurrentIndex = (CurrentIndex + 1) % _database.Characters.Count;
        Log.Info($"[CharacterSelect] Next → {_current.Id}");
        UpdateView();
    }

    public void Previous()
    {
        if (_database == null || _database.Characters == null || _database.Characters.Count == 0)
            return;

        CurrentIndex = (CurrentIndex - 1 + _database.Characters.Count) % _database.Characters.Count;
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