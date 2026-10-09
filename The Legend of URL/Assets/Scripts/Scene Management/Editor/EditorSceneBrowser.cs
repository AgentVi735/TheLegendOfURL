using UnityEditor;
using UnityEditor.SceneManagement;

internal static class EditorSceneBrowser
{
    [MenuItem("Scenes/Main Menu")]
    private static void OpenMainMenu() => OpenScene("Assets/Scenes/Main Menu.unity");
    
    [MenuItem("Scenes/Maps/Village")]
    private static void OpenVillage() => OpenScene("Assets/Scenes/Final Maps/Village Final.unity");
    [MenuItem("Scenes/Maps/Ranch")]
    private static void OpenRanch() => OpenScene("Assets/Scenes/Final Maps/Ranch Final.unity");
    [MenuItem("Scenes/Maps/Forest")]
    private static void OpenForest() => OpenScene("Assets/Scenes/Final Maps/Forest House Final.unity");
    [MenuItem("Scenes/Maps/Ravine")]
    private static void OpenRavine() => OpenScene("Assets/Scenes/Final Maps/Ravine Bridge Final.unity");
    
    [MenuItem("Scenes/Editor/Village")]
    private static void OpenEditorVillage() => OpenScene("Assets/Scenes/Maps/Village.unity");
    [MenuItem("Scenes/Editor/Ranch")]
    private static void OpenEditorRanch() => OpenScene("Assets/Scenes/Maps/Ranch.unity");
    [MenuItem("Scenes/Editor/Forest")]
    private static void OpenEditorForest() => OpenScene("Assets/Scenes/Maps/Forest House.unity");
    [MenuItem("Scenes/Editor/Ravine")]
    private static void OpenEditorRavine() => OpenScene("Assets/Scenes/Maps/Ravine Bridge.unity");
    
    [MenuItem("Scenes/Vicky/Main")]
    private static void OpenVickyMain() => OpenScene("Assets/Scenes/Vicky.unity");
    [MenuItem("Scenes/Vicky/Boss")]
    private static void OpenVickyBoss() => OpenScene("Assets/Scenes/Boss Test.unity");

    private static void OpenScene(string scenePath)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
    }
}