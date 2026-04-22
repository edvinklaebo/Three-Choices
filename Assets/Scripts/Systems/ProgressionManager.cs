using System;
using System.Collections.Generic;

using Core.Progression;

using UnityEngine;

namespace Systems
{
    /// <summary>
    /// Runtime metaprogression service.
    /// Tracks total points, unlocks entries from <see cref="ProgressionConfig"/>,
    /// and persists data through PlayerPrefs.
    /// </summary>
    public class ProgressionManager
    {
        private const string DefaultSaveKey = "meta_progression_state";
        private const int AutoSavePointInterval = 10;

        [Serializable]
        private class SaveData
        {
            public int totalPoints;
            public List<string> unlockedIds = new();
        }

        private readonly ProgressionConfig _config;
        private readonly string _saveKey;
        private readonly List<ProgressionUnlockDefinition> _sortedUnlocks = new();
        private readonly Dictionary<string, ProgressionUnlockDefinition> _unlockById = new();
        private readonly HashSet<string> _unlockedIds = new();

        public event Action<int> PointsChanged;
        public event Action<ProgressionUnlockDefinition> UnlockAchieved;

        public int TotalPoints => _totalPoints;
        public int ExpectedUnlockCount => _config.ExpectedUnlockCount;
        public int UnlockedCount => _unlockedIds.Count;
        public IReadOnlyCollection<string> UnlockedIds => _unlockedIds;

        private int _totalPoints;
        private int _unsavedPointDelta;

        public ProgressionManager(ProgressionConfig config, string saveKey = DefaultSaveKey)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _saveKey = string.IsNullOrWhiteSpace(saveKey) ? DefaultSaveKey : saveKey;

            BuildLookupTables();
            Debug.Assert(_unlockById.Count == _config.Unlocks.Count,
                $"[ProgressionManager] Config contains invalid unlock entries. " +
                $"Expected {_config.Unlocks.Count}, loaded {_unlockById.Count}.");
            Debug.Assert(_config.IsValid(),
                "[ProgressionManager] ProgressionConfig validation failed. Check unlock count and duplicate IDs.");
        }

        /// <summary>
        /// Call this whenever enemies are killed.
        /// 1 kill = 1 progression point by default.
        /// </summary>
        public void OnEnemyKilled(int amount = 1)
        {
            if (amount <= 0)
                return;

            AddPoints(amount);
        }

        public bool IsUnlocked(string unlockId)
        {
            return !string.IsNullOrWhiteSpace(unlockId) && _unlockedIds.Contains(unlockId);
        }

        public bool HasUnlockedEverything()
        {
            return _unlockedIds.Count >= _config.ExpectedUnlockCount;
        }

        public void LoadProgress()
        {
            _unlockedIds.Clear();
            _totalPoints = 0;
            _unsavedPointDelta = 0;

            if (!PlayerPrefs.HasKey(_saveKey))
                return;

            var json = PlayerPrefs.GetString(_saveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
                return;

            var loaded = JsonUtility.FromJson<SaveData>(json);
            if (loaded == null)
                return;

            _totalPoints = Mathf.Max(0, loaded.totalPoints);

            if (loaded.unlockedIds != null)
            {
                for (var i = 0; i < loaded.unlockedIds.Count; i++)
                {
                    var unlockId = loaded.unlockedIds[i];
                    if (_unlockById.ContainsKey(unlockId))
                        _unlockedIds.Add(unlockId);
                }
            }
        }

        public void SaveProgress()
        {
            var data = new SaveData
            {
                totalPoints = _totalPoints,
                unlockedIds = new List<string>(_unlockedIds)
            };

            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(_saveKey, json);
            PlayerPrefs.Save();
            _unsavedPointDelta = 0;
        }

        public void ResetProgress(bool deleteSave = true)
        {
            _totalPoints = 0;
            _unlockedIds.Clear();
            _unsavedPointDelta = 0;

            if (!deleteSave)
                return;

            PlayerPrefs.DeleteKey(_saveKey);
            PlayerPrefs.Save();
        }

        public int GetPointsNeededForUnlock(string unlockId)
        {
            if (!_unlockById.TryGetValue(unlockId, out var unlock))
                return -1;

            return Mathf.Max(0, unlock.UnlockCost - _totalPoints);
        }

        private void AddPoints(int points)
        {
            _totalPoints += points;
            _unsavedPointDelta += points;
            PointsChanged?.Invoke(_totalPoints);
            var unlockedAny = CheckUnlocks();

            if (unlockedAny || _unsavedPointDelta >= AutoSavePointInterval || HasUnlockedEverything())
                SaveProgress();
        }

        private bool CheckUnlocks()
        {
            var unlockedAny = false;
            for (var i = 0; i < _sortedUnlocks.Count; i++)
            {
                var unlock = _sortedUnlocks[i];
                if (_unlockedIds.Contains(unlock.Id))
                    continue;

                if (_totalPoints < unlock.UnlockCost)
                    continue;

                if (!AreDependenciesMet(unlock))
                    continue;

                _unlockedIds.Add(unlock.Id);
                unlockedAny = true;
                UnlockAchieved?.Invoke(unlock);
            }

            return unlockedAny;
        }

        private bool AreDependenciesMet(ProgressionUnlockDefinition unlock)
        {
            var dependencies = unlock.Dependencies;
            if (dependencies == null || dependencies.Count == 0)
                return true;

            for (var i = 0; i < dependencies.Count; i++)
            {
                if (!_unlockedIds.Contains(dependencies[i]))
                    return false;
            }

            return true;
        }

        private void BuildLookupTables()
        {
            _unlockById.Clear();
            _sortedUnlocks.Clear();

            var unlocks = _config.Unlocks;
            for (var i = 0; i < unlocks.Count; i++)
            {
                var unlock = unlocks[i];
                if (unlock == null || string.IsNullOrWhiteSpace(unlock.Id))
                    continue;

                if (_unlockById.ContainsKey(unlock.Id))
                    continue;

                _unlockById.Add(unlock.Id, unlock);
                _sortedUnlocks.Add(unlock);
            }

            _sortedUnlocks.Sort(static (left, right) => left.UnlockCost.CompareTo(right.UnlockCost));
        }
    }
}
