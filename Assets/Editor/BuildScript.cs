using System;
using UnityEditor;

public class BuildScript
{
    static void Build_Android()
    {
        try
        {
            var now = DateTime.Now;
            var major = int.Parse(now.ToString("yyyy"));
            var minor = int.Parse(now.ToString("MMdd"));
            var build = int.Parse(now.ToString("HHmm"));
            PlayerSettings.bundleVersion = major + "." + minor + "." + build;
            PlayerSettings.Android.bundleVersionCode = build;
            UnityEngine.Debug.Log("Finished with bundleversioncode:" + PlayerSettings.Android.bundleVersionCode +
                                  " and version" + PlayerSettings.bundleVersion);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e);
            UnityEngine.Debug.LogError(
                "AutoIncrementBuildVersion script failed. Make sure your current bundle version is in the format X.X.X (e.g. 1.0.0) and not X.X (1.0) or X (1).");
            throw;
        }
        BuildPlayerOptions buildPlayerOptions =
            new BuildPlayerOptions
            {
                scenes = new[]
                {
                    "Assets/Scenes/UserRegistration.unity", "Assets/Scenes/MainMenu.unity",
                    "Assets/Scenes/Tutorial.unity", "Assets/Scenes/mainGame.unity"
                },
                locationPathName = "bin/numeric_game.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildPlayerOptions buildPlayerOptionsDev =
            new BuildPlayerOptions
            {
                scenes = new[]
                {
                    "Assets/Scenes/UserRegistration.unity", "Assets/Scenes/MainMenu.unity",
                    "Assets/Scenes/Tutorial.unity", "Assets/Scenes/mainGame.unity"
                },
                locationPathName = "bin/numeric_game_dev.apk",
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };
        BuildPipeline.BuildPlayer(buildPlayerOptionsDev);
    }
}