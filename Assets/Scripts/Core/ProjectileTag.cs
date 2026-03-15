using UnityEngine;

namespace Core
{
    /// <summary>
    ///     Marker component that identifies a GameObject as a projectile.
    ///     Attach to any projectile object (fireball, arcane missile, etc.) so that
    ///     other systems can detect projectiles via GetComponent&lt;ProjectileTag&gt;()
    ///     rather than relying on string tags or type checks.
    /// </summary>
    public class ProjectileTag : MonoBehaviour
    {
    }
}
