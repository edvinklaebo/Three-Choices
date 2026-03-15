using Core;

using NUnit.Framework;

using Systems;

using UnityEngine;

namespace Tests.EditModeTests
{
    public class ProjectileTagTests
    {
        [Test]
        public void ProjectileTag_CanBeAddedToGameObject()
        {
            var go = new GameObject("TestProjectile");
            var tag = go.AddComponent<ProjectileTag>();

            Assert.IsNotNull(tag, "ProjectileTag should be addable to a GameObject");
            Assert.IsInstanceOf<MonoBehaviour>(tag, "ProjectileTag should be a MonoBehaviour");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void AnimationService_SetProjectile_AddsProjectileTag_WhenNotPresent()
        {
            var go = new GameObject("TestProjectile");
            go.AddComponent<SpriteRenderer>();

            var service = new AnimationService();
            service.SetProjectile(go.transform);

            Assert.IsNotNull(go.GetComponent<ProjectileTag>(),
                "SetProjectile should add ProjectileTag to projectile GameObject");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void AnimationService_SetProjectile_DoesNotDuplicateProjectileTag_WhenAlreadyPresent()
        {
            var go = new GameObject("TestProjectile");
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<ProjectileTag>();

            var service = new AnimationService();
            service.SetProjectile(go.transform);

            var tags = go.GetComponents<ProjectileTag>();
            Assert.AreEqual(1, tags.Length,
                "SetProjectile should not add a duplicate ProjectileTag if one is already present");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void AnimationService_SetProjectile_Null_DoesNotThrow()
        {
            var service = new AnimationService();
            Assert.DoesNotThrow(() => service.SetProjectile(null),
                "SetProjectile(null) should not throw");
        }
    }
}
