using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Scene Holder")]
public class SceneHolder : ScriptableObject
{
#if UNITY_EDITOR
    [SerializeField] private SceneAsset[] scenes = Array.Empty<SceneAsset>();
#endif
    [SerializeField] private string[] sceneNames;

#if UNITY_EDITOR
    private void OnValidate()
    {
        List<string> list = new();
        try
        {
            foreach (SceneAsset scene in scenes)
            {
                if (!list.Contains(scene.name))
                    list.Add(scene.name);
            }
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