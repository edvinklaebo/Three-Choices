using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Injectable ScriptableObject implementation of ICombatSystem.
/// Assign in the Inspector to decouple CombatController from the static CombatSystem.
/// </summary>
[CreateAssetMenu(menuName = "Systems/Combat System Service")]
public class CombatSystemService : ScriptableObject, ICombatSystem
{
    public List<ICombatAction> RunFight(Unit attacker, Unit defender)
    {
        var engine = new CombatEngine();
        return engine.RunFight(attacker, defender);
    }
}
