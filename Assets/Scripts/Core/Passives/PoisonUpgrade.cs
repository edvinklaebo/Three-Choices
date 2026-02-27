using System;
using UnityEngine;

[Serializable]
public class PoisonUpgrade : IPassive
{
    [SerializeField] private int _stacks;
    [SerializeField] private int _duration;
    [SerializeField] private int _baseDamage;

    public PoisonUpgrade(Unit owner, int stacks = 2, int duration = 3, int baseDamage = 2)
    {
        _stacks = stacks;
        _duration = duration;
        _baseDamage = baseDamage;
    }

    public void OnAttach(Unit owner)
    {
        owner.OnHit += ApplyPoison;
    }

    public void OnDetach(Unit owner)
    {
        owner.OnHit -= ApplyPoison;
    }

    private void ApplyPoison(Unit self, Unit target, int _)
    {
        if (target == null)
            return;

        Log.Info("Poison passive triggered", new
        {
            target = target.Name,
            stacks = _stacks,
            duration = _duration,
            baseDamage = _baseDamage
        });

        target.ApplyStatus(new Poison(_stacks, _duration, _baseDamage));
    }
}
