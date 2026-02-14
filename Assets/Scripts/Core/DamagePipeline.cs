using System.Collections.Generic;

public static class DamagePipeline
{
    private static readonly List<IDamageModifier> modifiers = new();

    public static void Register(IDamageModifier mod)
    {
        modifiers.Add(mod);
    }

    public static void Unregister(IDamageModifier mod)
    {
        modifiers.Remove(mod);
    }

    public static void Process(DamageContext ctx)
    {
        foreach (var mod in modifiers)
            mod.Modify(ctx);

        if (ctx.FinalValue < 0)
            ctx.FinalValue = 0;
    }
}