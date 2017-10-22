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
                locationPathName = "apk/numerical_game.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
}