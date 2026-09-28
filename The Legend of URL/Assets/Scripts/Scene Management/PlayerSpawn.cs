using UnityEditor;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    public LoadZoneData _data => data;
    [SerializeField] private LoadZoneData data;
    public bool IsDefault;
    [SerializeField] private bool update;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (update)
        {
            if (!Application.isPlaying)
                UpdateData();
            else
                Debug.LogError($"Unable to update {data.name} while application is playing. Please exit the application to update {data.name}.");
        }
    }

    private void UpdateData()
    {
        if (data == null) return;

        data.sceneIdx = gameObject.scene.buildIndex;
        data.newPosition = transform.position;
        data.newRotation = transform.rotation.eulerAngles;
        data.cameraRotation = data.newRotation.y;
        update = false;
        EditorUtility.SetDirty(data);
        Debug.Log($"Updated {data.name} with data from {gameObject.name} in scene {gameObject.scene.name}.");
    }
#endif
}