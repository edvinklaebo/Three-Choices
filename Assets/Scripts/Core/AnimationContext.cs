/// <summary>
/// Centralized presentation access for combat animations.
/// Keeps actions decoupled from scene objects.
/// </summary>
public class AnimationContext
{
    public AnimationService Anim { get; }
    public UIService UI { get; }
    public VFXService VFX { get; }
    public SFXService SFX { get; }

    public AnimationContext(AnimationService anim, UIService ui, VFXService vfx, SFXService sfx)
    {
        Anim = anim;
        UI = ui;
        VFX = vfx;
        SFX = sfx;
    }
}
