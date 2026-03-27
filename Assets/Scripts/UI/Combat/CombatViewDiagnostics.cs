using System.Text;

using Controllers;

using UnityEngine;

using Utils;

namespace UI.Combat
{
    /// <summary>
    /// Diagnostic tool to check Combat View integration.
    /// Attach this to any GameObject in the scene and it will report integration status.
    /// </summary>
    public class CombatViewDiagnostics : MonoBehaviour
    {
        [Header("Run Diagnostics")]
        [SerializeField] private bool _runOnStart = true;
    
        private void Start()
        {
            if (_runOnStart)
            {
                RunDiagnostics();
            }
        }

        [ContextMenu("Run Diagnostics")]
        public void RunDiagnostics()
        {
            var report = new StringBuilder();
            report.AppendLine("=== COMBAT VIEW INTEGRATION DIAGNOSTICS ===\n");

            // Check CombatView
            var combatView = FindFirstObjectByType<CombatView>();
            if (combatView == null)
            {
                report.AppendLine("❌ CRITICAL: CombatView not found in scene!");
                report.AppendLine("   → Add CombatView component to a GameObject in your scene\n");
            }
            else
            {
                report.AppendLine("✓ CombatView found");
            
                // Check PlayerView
                if (combatView.PlayerView == null)
                {
                    report.AppendLine("❌ CRITICAL: PlayerView not assigned in CombatView");
                    report.AppendLine("   → Assign PlayerView in CombatView inspector\n");
                }
                else
                {
                    report.AppendLine("  ✓ PlayerView assigned");
                    CheckUnitView(combatView.PlayerView, "Player", report);
                }

                // Check EnemyView
                if (combatView.EnemyView == null)
                {
                    report.AppendLine("❌ CRITICAL: EnemyView not assigned in CombatView");
                    report.AppendLine("   → Assign EnemyView in CombatView inspector\n");
                }
                else
                {
                    report.AppendLine("  ✓ EnemyView assigned");
                    CheckUnitView(combatView.EnemyView, "Enemy", report);
                }
            }

            // Check FloatingTextPool
            var floatingTextPool = FindFirstObjectByType<FloatingTextPool>();
            if (floatingTextPool == null)
            {
                report.AppendLine("❌ CRITICAL: FloatingTextPool not found in scene!");
                report.AppendLine("   → Create GameObject with FloatingTextPool component");
                report.AppendLine("   → This is why damage numbers aren't displaying\n");
            }
            else
            {
                report.AppendLine("✓ FloatingTextPool found");
            
                // Check prefab assignment using reflection
                var prefabField = typeof(FloatingTextPool).GetField("_floatingTextPrefab", 
                                                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var prefab = prefabField?.GetValue(floatingTextPool);
            
                if (prefab == null)
                {
                    report.AppendLine("  ❌ FloatingText prefab not assigned!");
                    report.AppendLine("     → Assign FloatingText prefab in FloatingTextPool inspector\n");
                }
                else
                {
                    report.AppendLine("  ✓ FloatingText prefab assigned");
                }
            }

            // Check CombatHUD
            var combatHUD = FindFirstObjectByType<CombatHUD>();
            if (combatHUD == null)
            {
                report.AppendLine("⚠️  CombatHUD not found in scene");
                report.AppendLine("   → Health bars and status effects won't display\n");
            }
            else
            {
                report.AppendLine("✓ CombatHUD found");
            }

            // Check TurnIndicatorUI
            var turnIndicator = FindFirstObjectByType<TurnIndicatorUI>();
            if (turnIndicator == null)
            {
                report.AppendLine("⚠️  TurnIndicatorUI not found in scene");
                report.AppendLine("   → This is why turn text isn't displaying");
                report.AppendLine("   → Create GameObject with TurnIndicatorUI component\n");
            }
            else
            {
                report.AppendLine("✓ TurnIndicatorUI found");
            }

            // Check CombatOrchestrator
            var combatOrchestrator = FindFirstObjectByType<CombatOrchestrator>();
            report.AppendLine(combatOrchestrator == null
                ? "⚠️  CombatOrchestrator not found in scene\n"
                : "✓ CombatOrchestrator found");

            // Check CombatPresentationCoordinator
            var presentationCoordinator = FindFirstObjectByType<CombatPresentationCoordinator>();
            report.AppendLine(presentationCoordinator == null
                ? "⚠️  CombatPresentationCoordinator not found in scene\n"
                : "✓ CombatPresentationCoordinator found");

            // Check GameFlowController
            var gameFlowController = FindFirstObjectByType<GameFlowController>();
            report.AppendLine(gameFlowController == null
                ? "⚠️  GameFlowController not found in scene\n"
                : "✓ GameFlowController found");

            report.AppendLine("\n=== END DIAGNOSTICS ===");
        
            Log.Info(report.ToString());
        }

        private void CheckUnitView(UnitView unitView, string componentName, StringBuilder report)
        {
            report.AppendLine(unitView.IdlePoint == null
                ? $"    ❌ {componentName} IdlePoint not assigned"
                : $"    ✓ {componentName} IdlePoint: {unitView.IdlePoint.localPosition}");

            report.AppendLine(unitView.LungePoint == null
                ? $"    ❌ {componentName} LungePoint not assigned"
                : $"    ✓ {componentName} LungePoint: {unitView.LungePoint.localPosition}");

            report.AppendLine(unitView.SpriteTransform == null
                ? $"    ⚠️  {componentName} has no SpriteRenderer child"
                : $"    ✓ {componentName} SpriteRenderer found");
        }
    }
}
