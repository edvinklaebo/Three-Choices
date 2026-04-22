using Core.Progression;

using Systems;

using UnityEngine;

using Utils;

namespace Controllers
{
    /// <summary>
    /// MonoBehaviour bootstrap that creates and owns the single <see cref="ProgressionManager"/>
    /// instance for the lifetime of the application.
    /// Persists across scene loads via <see cref="Object.DontDestroyOnLoad"/>.
    ///
    /// Place this on a root GameObject in the first loaded scene (e.g. MainMenu).
    /// Access progression from anywhere via <see cref="Progression"/>.
    /// </summary>
    public class GameServices : MonoBehaviour
    {
        public static ProgressionManager Progression { get; private set; }

        [SerializeField] private ProgressionConfig _config;

        private bool _isPrimaryInstance;

        private void Awake()
        {
            if (Progression != null)
            {
                Log.Warning("[GameServices] Duplicate instance detected — destroying this GameObject. " +
                            "Ensure only one GameServices exists in the first loaded scene.");
                Destroy(gameObject);
                return;
            }

            if (_config == null)
            {
                Log.Error("[GameServices] ProgressionConfig is not assigned. Assign it in the Inspector.");
                return;
            }

            DontDestroyOnLoad(gameObject);
            _isPrimaryInstance = true;

            Progression = new ProgressionManager(_config);
            Progression.Initialize(loadSavedProgress: true);
        }

        private void OnDestroy()
        {
            if (_isPrimaryInstance)
                Progression?.SaveProgress();
        }
    }
}
