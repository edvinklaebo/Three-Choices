using System.Linq;

using UnityEditor;

namespace Editor
{
    public static class BuildScript
    {
        private static string[] Scenes =>
            EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

        public static void BuildWindows()
        {
            BuildPipeline.BuildPlayer(
                Scenes,
                "Builds/Windows/MyGame.exe",
                BuildTarget.StandaloneWindows64,
                BuildOptions.None
            );
        }

        public static void BuildLinux()
        {
            BuildPipeline.BuildPlayer(
                Scenes,
                "Builds/Linux/MyGame.x86_64",
                BuildTarget.StandaloneLinux64,
                BuildOptions.None
            );
        }

        public static void BuildMac()
        {
            BuildPipeline.BuildPlayer(
                Scenes,
                "Builds/macOS/MyGame.app",
                BuildTarget.StandaloneOSX,
                BuildOptions.None
            );
        }

        public static void BuildAndroid()
        {
            BuildPipeline.BuildPlayer(
                Scenes,
                "Builds/Android/MyGame.apk",
                BuildTarget.Android,
                BuildOptions.None
            );
        }

        public static void BuildIOS()
        {
            BuildPipeline.BuildPlayer(
                Scenes,
                "Builds/iOS",
                BuildTarget.iOS,
                BuildOptions.None
            );
        }
    }
}