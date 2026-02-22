/// <summary>
/// Centralized presentation access for combat animations.
/// Keeps actions decoupled from scene objects.
/// </summary>
public class AnimationContext
{
    public AnimationService AnimationService { get; }
    public UIService UIService { get; }
    public VFXService VFXService { get; }
    public SFXService SFXService { get; }

    public AnimationContext(AnimationService animationService, UIService uiService, VFXService vfxService, SFXService sfxService)
    {
        AnimationService = animationService;
        UIService = uiService;
        VFXService = vfxService;
        SFXService = sfxService;
    }
}
