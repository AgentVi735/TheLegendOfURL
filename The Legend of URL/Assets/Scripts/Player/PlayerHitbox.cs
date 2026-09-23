using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    [SerializeField] private SceneController sceneController;
    [SerializeField] private PlayerController playerController;
    
    [SerializeField] private string loadZoneTag;
    
    private void OnTriggerEnter(Collider collider)
    {
        if (!collider.CompareTag(loadZoneTag)) return;
        LoadZoneData data = collider.GetComponent<LoadZone>()._data;
        sceneController.LoadSceneFromData(data);
    }
}