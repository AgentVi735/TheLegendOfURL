using UnityEngine;

[CreateAssetMenu(menuName = "Load Zone Data")]
public class LoadZoneData : ScriptableObject
{
    public int sceneIdx = -1;
    public Vector3 newPosition;
    public Vector3 newRotation;
    public float cameraRotation;
}