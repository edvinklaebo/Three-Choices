/// <summary>
///     Creates player <see cref="Unit" /> instances from character definitions.
/// </summary>
public static class PlayerFactory
{
    public static Unit CreateFromCharacter(CharacterDefinition character)
    {
        if (character == null)
        {
            Log.Error("[PlayerFactory] Cannot create player from null character");
            return null;
        }

        return new Unit(character.DisplayName)
        {
            Stats = new Stats
            {
                Armor = character.Armor,
                AttackPower = character.Attack,
                CurrentHP = character.MaxHp,
                MaxHP = character.MaxHp,
                Speed = character.Speed
            }
        };
    }
}
