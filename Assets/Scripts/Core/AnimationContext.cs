/// <summary>
/// Centralized presentation access for combat animations.
/// Keeps actions decoupled from scene objects.
/// </summary>
public class AnimationContext
{
    public AnimationService Anim { get; set; }
    public UIService UI { get; set; }
    public VFXService VFX { get; set; }
    public SFXService SFX { get; set; }

    public AnimationContext(AnimationService anim, UIService ui, VFXService vfx, SFXService sfx)
    {
        Anim = anim;
        UI = ui;
        VFX = vfx;
        SFX = sfx;
    }
}
