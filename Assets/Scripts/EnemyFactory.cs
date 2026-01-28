public static class EnemyFactory
{
    public static Unit Create(int fightIndex)
    {
        try
        {
            var unitName = MonsterName.Random();
            var maxHP = 15 + fightIndex * 5;
            var attackPower = 3 + fightIndex;
            var armor = 2 + fightIndex / 2;
            var speed = 5 + fightIndex / 2;

            var unit = new Unit(unitName)
            {
                Stats = new Stats
                {
                    MaxHP = maxHP,
                    CurrentHP = maxHP,
                    AttackPower = attackPower,
                    Armor = armor,
                    Speed = speed
                }
            };

            Log.Info("Enemy created", new
            {
                fightIndex,
                unitName,
                maxHP,
                attackPower,
                armor,
                speed
            });

            return unit;
        }
        catch (System.Exception ex)
        {
            Log.Exception(ex, "EnemyFactory.Create failed", new { fightIndex });
            throw;
        }
    }
}