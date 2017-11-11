using UnityEditor;

public class BuildScript
{
    static void Build_Android()
    {
        BuildPlayerOptions buildPlayerOptions =
            new BuildPlayerOptions
            {
                scenes = new[]
                {
                    "Assets/Scenes/UserRegistration.unity", "Assets/Scenes/MainMenu.unity",
                    "Assets/Scenes/Tutorial.unity", "Assets/Scenes/mainGame.unity"
                },
                locationPathName = "bin/numerical_game.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildPlayerOptions buildPlayerOptions_dev =
            new BuildPlayerOptions
            {
                scenes = new[]
                {
                    "Assets/Scenes/UserRegistration.unity", "Assets/Scenes/MainMenu.unity",
                    "Assets/Scenes/Tutorial.unity", "Assets/Scenes/mainGame.unity"
                },
                locationPathName = "bin/numerical_game_dev.apk",
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };
        BuildPipeline.BuildPlayer(buildPlayerOptions_dev);
    }
}