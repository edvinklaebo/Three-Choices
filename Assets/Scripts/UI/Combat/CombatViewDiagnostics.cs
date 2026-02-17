using UnityEngine;
using System.Text;

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

        // Check CombatController
        var combatController = FindFirstObjectByType<CombatController>();
        if (combatController == null)
        {
            report.AppendLine("⚠️  CombatController not found in scene\n");
        }
        else
        {
            report.AppendLine("✓ CombatController found");
            
            // Check if CombatView is wired
            var viewField = typeof(CombatController).GetField("combatView",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var assignedView = viewField?.GetValue(combatController);
            
            if (assignedView == null)
            {
                report.AppendLine("  ⚠️  CombatView not assigned in CombatController");
                report.AppendLine("     → Will auto-find but better to assign explicitly\n");
            }
            else
            {
                report.AppendLine("  ✓ CombatView assigned in CombatController");
            }
        }

        report.AppendLine("\n=== END DIAGNOSTICS ===");
        
        Log.Info(report.ToString());
    }

    private void CheckUnitView(UnitView unitView, string componentName, StringBuilder report)
    {
        if (unitView.IdlePoint == null)
        {
            report.AppendLine($"    ❌ {componentName} IdlePoint not assigned");
        }
        else
        {
            report.AppendLine($"    ✓ {componentName} IdlePoint: {unitView.IdlePoint.localPosition}");
        }

        if (unitView.LungePoint == null)
        {
            report.AppendLine($"    ❌ {componentName} LungePoint not assigned");
        }
        else
        {
            report.AppendLine($"    ✓ {componentName} LungePoint: {unitView.LungePoint.localPosition}");
        }

        if (unitView.SpriteTransform == null)
        {
            report.AppendLine($"    ⚠️  {componentName} has no SpriteRenderer child");
        }
        else
        {
            report.AppendLine($"    ✓ {componentName} SpriteRenderer found");
        }
    }
}
