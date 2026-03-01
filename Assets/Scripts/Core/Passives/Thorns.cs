using System;

/// <summary>
/// Deals damage equal to half the owner's armour to any unit that attacks them.
/// Reflects only on direct attacks (attacker != null) to avoid triggering on status effects.
/// Uses a re-entrance guard to prevent infinite reflect chains when both units carry Thorns.
/// </summary>
[Serializable]
public class Thorns : IPassive
{
    [NonSerialized] private bool _isReflecting;

    public int Priority => 100;

    public void OnAttach(Unit owner)
    {
        owner.Damaged += OnDamaged;
    }

    public void OnDetach(Unit owner)
    {
        owner.Damaged -= OnDamaged;
    }

    private void OnDamaged(Unit self, Unit attacker, int damageTaken)
    {
        if (attacker == null)
            return;

        if (_isReflecting)
            return;

        var thornsDamage = self.Stats.Armor / 2;
        if (thornsDamage <= 0)
            return;

        Log.Info($"Thorns reflect: {self.Name} dealt {thornsDamage} to {attacker.Name}");

        _isReflecting = true;
        try
        {
            attacker.ApplyDamage(self, thornsDamage);
        }
        finally
        {
            _isReflecting = false;
        }
    }
}