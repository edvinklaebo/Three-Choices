using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class PlatformUtilsTests
    {
        [Test]
        public void IsQuitSupported_ReturnsTrue_InEditor()
        {
            // In the Unity Editor the method always returns true (compile-time constant).
            Assert.IsTrue(PlatformUtils.IsQuitSupported());
        }

        [Test]
        public void IsQuitSupportedOnPlatform_ReturnsTrue_ForWindowsPlayer()
        {
            Assert.IsTrue(PlatformUtils.IsQuitSupportedOnPlatform(RuntimePlatform.WindowsPlayer));
        }

        [Test]
        public void IsQuitSupportedOnPlatform_ReturnsTrue_ForLinuxPlayer()
        {
            Assert.IsTrue(PlatformUtils.IsQuitSupportedOnPlatform(RuntimePlatform.LinuxPlayer));
        }

        [Test]
        public void IsQuitSupportedOnPlatform_ReturnsTrue_ForOSXPlayer()
        {
            Assert.IsTrue(PlatformUtils.IsQuitSupportedOnPlatform(RuntimePlatform.OSXPlayer));
        }

        [Test]
        public void IsQuitSupportedOnPlatform_ReturnsFalse_ForWebGL()
        {
            Assert.IsFalse(PlatformUtils.IsQuitSupportedOnPlatform(RuntimePlatform.WebGLPlayer));
        }

        [Test]
        public void IsQuitSupportedOnPlatform_ReturnsFalse_ForIOS()
        {
            Assert.IsFalse(PlatformUtils.IsQuitSupportedOnPlatform(RuntimePlatform.IPhonePlayer));
        }

        [Test]
        public void IsQuitSupportedOnPlatform_ReturnsFalse_ForAndroid()
        {
            Assert.IsFalse(PlatformUtils.IsQuitSupportedOnPlatform(RuntimePlatform.Android));
        }

        [Test]
        public void IsQuitSupportedOnPlatform_ReturnsFalse_ForPS4()
        {
            Assert.IsFalse(PlatformUtils.IsQuitSupportedOnPlatform(RuntimePlatform.PS4));
        }

        [Test]
        public void IsQuitSupportedOnPlatform_ReturnsFalse_ForSwitch()
        {
            Assert.IsFalse(PlatformUtils.IsQuitSupportedOnPlatform(RuntimePlatform.Switch));
        }

        [Test]
        public void IsQuitSupportedOnPlatform_ReturnsFalse_ForXboxOne()
        {
            Assert.IsFalse(PlatformUtils.IsQuitSupportedOnPlatform(RuntimePlatform.XboxOne));
        }

        [Test]
        public void IsQuitSupportedOnPlatform_ReturnsFalse_ForTvOS()
        {
            Assert.IsFalse(PlatformUtils.IsQuitSupportedOnPlatform(RuntimePlatform.tvOS));
        }
    }
}
