using UnityEngine;

public class CharacterSelectController : MonoBehaviour
{
    [SerializeField] private CharacterDatabase _database;
    [SerializeField] private CharacterSelectView _view;

    private int _currentIndex;

    public int CurrentIndex => _currentIndex;

    private void OnEnable()
    {
        UpdateView();
    }

    public void Next()
    {
        if (_database == null || _database.Characters == null || _database.Characters.Count == 0)
            return;

        _currentIndex = (_currentIndex + 1) % _database.Characters.Count;
        Debug.Log($"[CharacterSelect] Next → {Current.Id}");
        UpdateView();
    }

    public void Previous()
    {
        if (_database == null || _database.Characters == null || _database.Characters.Count == 0)
            return;

        _currentIndex = (_currentIndex - 1 + _database.Characters.Count) % _database.Characters.Count;
        Debug.Log($"[CharacterSelect] Prev → {Current.Id}");
        UpdateView();
    }

    public void Confirm()
    {
        if (Current == null)
        {
            Debug.LogError("[CharacterSelect] Cannot confirm - no character selected");
            return;
        }

        Debug.Log($"[CharacterSelect] Confirmed {Current.Id}");
        GameEvents.CharacterSelected_Event?.Invoke(Current);

        var runController = FindFirstObjectByType<RunController>();
        if (runController != null)
        {
            runController.StartNewRun(Current);
        }
        else
        {
            Debug.LogError("[CharacterSelect] RunController not found");
        }
    }

    public CharacterDefinition Current => _database?.GetByIndex(_currentIndex);

    private void UpdateView()
    {
        if (_view != null && Current != null)
        {
            _view.DisplayCharacter(Current);
        }
    }
}
