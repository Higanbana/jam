using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayModeStartSceneBootstrap
{
    private const string MainScenePath = "Assets/Scenes/Main.unity";

    static PlayModeStartSceneBootstrap()
    {
        SceneAsset mainScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath);
        if (mainScene != null)
        {
            EditorSceneManager.playModeStartScene = mainScene;
        }
    }
}
