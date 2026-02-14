using UnityEngine;

public class CharacterSelectController : MonoBehaviour
{
    [SerializeField] private CharacterDatabase _database;
    [SerializeField] private CharacterSelectView _view;

    private RunController _runController;

    public int CurrentIndex { get; private set; }

    private CharacterDefinition _current => _database?.GetByIndex(CurrentIndex);

    private void Awake()
    {
        // Cache RunController reference to avoid repeated scene searches
        _runController = FindFirstObjectByType<RunController>();
        if (_runController == null) 
            Log.Error("[CharacterSelect] RunController not found in scene");
    }

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
        Debug.Log($"[CharacterSelect] Prev → {_current.Id}");
        UpdateView();
    }

    public void Confirm()
    {
        if (_current == null)
        {
            Debug.LogError("[CharacterSelect] Cannot confirm - no character selected");
            return;
        }

        Debug.Log($"[CharacterSelect] Confirmed {_current.Id}");
        GameEvents.CharacterSelected_Event?.Invoke(_current);

        if (_runController != null)
            _runController.StartNewRun(_current);
        else if (Application.isPlaying)
            Log.Error("[CharacterSelect] RunController not found");
        else
            Log.Info("[CharacterSelect] RunController not found");
    }

    private void UpdateView()
    {
        if (_view != null && _current != null) _view.DisplayCharacter(_current);
    }
}