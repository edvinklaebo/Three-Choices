using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class CombatViewPresenterTests
    {
        [Test]
        public void CombatViewPresenter_ImplementsICombatViewPresenter()
        {
            var go = new GameObject("TestCombatViewPresenter");
            var presenter = go.AddComponent<CombatViewPresenter>();

            Assert.IsInstanceOf<ICombatViewPresenter>(presenter);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Context_ReturnsNull_WhenServicesInstallerNotAssigned()
        {
            var go = new GameObject("TestCombatViewPresenter");
            var presenter = go.AddComponent<CombatViewPresenter>();

            // _servicesInstaller is null — Context should return null without throwing
            Assert.DoesNotThrow(() =>
            {
                var ctx = presenter.Context;
                Assert.IsNull(ctx);
            });

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Hide_DoesNotThrow_WhenServicesInstallerNotAssigned()
        {
            var go = new GameObject("TestCombatViewPresenter");
            var presenter = go.AddComponent<CombatViewPresenter>();

            // Null-safe: should not throw even with no installer
            Assert.DoesNotThrow(() => presenter.Hide());

            Object.DestroyImmediate(go);
        }
    }
}
