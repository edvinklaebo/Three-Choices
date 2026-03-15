using System;

using Systems;

namespace Core
{
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
            Anim = anim ?? throw new ArgumentNullException(nameof(anim));
            UI = ui ?? throw new ArgumentNullException(nameof(ui));
            VFX = vfx ?? throw new ArgumentNullException(nameof(vfx));
            SFX = sfx ?? throw new ArgumentNullException(nameof(sfx));
        }
    }
}
