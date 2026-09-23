using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    public LoadZoneData _data => data;
    [SerializeField] private LoadZoneData data;
    public bool IsDefault;
    [SerializeField] private bool automaticallyUpdate;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying && automaticallyUpdate)
            UpdateData();
    }

    private void UpdateData()
    {
        if (data == null) return;

        data.sceneIdx = gameObject.scene.buildIndex;
        data.newPosition = transform.position;
        data.newRotation = transform.rotation.eulerAngles;
        data.cameraRotation = data.newRotation.y;
        Debug.Log($"Updated {data.name} with data from {gameObject.name} in scene {gameObject.scene.name}.");
    }
#endif
}