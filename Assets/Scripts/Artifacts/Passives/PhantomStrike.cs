using System;
using UnityEngine;

/// <summary>
/// Crown of Echoes effect.
/// Every N hits on enemies triggers a phantom strike dealing a percentage of the triggering hit's damage.
/// The phantom strike bypasses armor — it is a spectral, unblockable hit.
/// </summary>
[Serializable]
public class PhantomStrike : IPassive
{
    [SerializeField] private int _hitsPerTrigger;
    [SerializeField] private float _damagePercent;
    [SerializeField] private int _hitCount;

    [NonSerialized] private Unit _owner;

    public int Priority => 100;

    public PhantomStrike(int hitsPerTrigger = 5, float damagePercent = 0.5f)
    {
        _hitsPerTrigger = hitsPerTrigger;
        _damagePercent = damagePercent;
    }

    public void OnAttach(Unit owner)
    {
        _owner = owner;
        owner.OnHit += OnHit;
    }

    public void OnDetach(Unit owner)
    {
        owner.OnHit -= OnHit;
        _owner = null;
    }

    private void OnHit(Unit self, Unit target, int damage)
    {
        if (target == null || target.IsDead)
            return;

        _hitCount++;

        if (_hitCount < _hitsPerTrigger)
            return;

        _hitCount = 0;
        var phantomDamage = Mathf.CeilToInt(damage * _damagePercent);

        Log.Info("[PhantomStrike] Phantom strike triggered", new
        {
            owner = self.Name,
            target = target.Name,
            phantomDamage
        });

        target.ApplyDamage(self, phantomDamage);
    }

    /// <summary>Current hit count toward the next phantom strike trigger.</summary>
    public int HitCount => _hitCount;
}
