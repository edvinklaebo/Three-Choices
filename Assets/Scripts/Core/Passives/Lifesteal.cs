using System;
using UnityEngine;

[Serializable]
public class Lifesteal : IPassive, ICombatListener
{
    [SerializeField] private float percent;
    private Unit _owner;

    public int Priority => 250; // Late priority - after damage is dealt

    public Lifesteal(Unit owner, float percent)
    {
        this.percent = percent;
    }

    public void OnAttach(Unit owner)
    {
        _owner = owner;
    }

    public void OnDetach(Unit owner)
    {
        _owner = null;
    }

    public void RegisterHandlers(CombatContext context)
    {
        context.On<DamagePhaseEvent>(OnDamagePhase);
    }

    public void UnregisterHandlers(CombatContext context)
    {
        context.Off<DamagePhaseEvent>(OnDamagePhase);
    }

    private void OnDamagePhase(DamagePhaseEvent evt)
    {
        if (evt.Phase != CombatPhase.PostResolve) return;
        if (evt.Context.Source != _owner) return;

        var healAmount = Mathf.CeilToInt(evt.Context.FinalDamage * percent);
        evt.Context.PendingHealing += healAmount;
    }
}