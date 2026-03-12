using Core;

using Systems;

using UI.Combat;

using UnityEngine;

using Utils;

namespace Controllers
{
    /// <summary>
    /// Bootstrapper that creates and wires all combat presentation services.
    /// Owns the AnimationContext so CombatPresentationCoordinator receives it ready-made.
    /// </summary>
    public class CombatServicesInstaller : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CombatView _combatView;

        public AnimationContext Context { get; private set; }
        public CombatView CombatView => this._combatView;

        private void Awake()
        {
            if (this._combatView == null)
            {
                Log.Error("CombatServicesInstaller: _combatView is not assigned. Assign it in the Inspector.");
                return;
            }

            var animService = new AnimationService();
            var uiService = new UIService();

            animService.SetCombatView(this._combatView);
            animService.SetProjectile(this._combatView.Projectile);

            Context = new AnimationContext(animService, uiService, new VFXService(), new SFXService());
        }
    }
}
