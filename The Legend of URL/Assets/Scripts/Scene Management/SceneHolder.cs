using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Scene Holder")]
public class SceneHolder : ScriptableObject
{
    public int MainMenuSceneIdx = 0;
    public int StartGameSceneIdx = 1;
#if UNITY_EDITOR
    [SerializeField] private SceneAsset[] scenes = Array.Empty<SceneAsset>();
#endif
    [SerializeField] private string[] sceneNames;

#if UNITY_EDITOR
    private void OnValidate()
    {
        List<string> list = new();
        List<EditorBuildSettingsScene> buildScenes = new();
        try
        {
            foreach (SceneAsset scene in scenes)
            {
                if (list.Contains(scene.name)) continue;
                list.Add(scene.name);
                buildScenes.Add(new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(scene), true));
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
        }
        catch
        {
            // this is here because of a unity issue
        }

        sceneNames = list.ToArray();
    }
#endif

    public string GetSceneName(int idx) => sceneNames[idx];
}