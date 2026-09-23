using UnityEngine;

public class GameInitialisation : MonoBehaviour
{
    [SerializeField] private SceneController sceneController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Vector3 playerSpawnPos;
    [SerializeField] private FadeManager fadeManager;

    private void Start()
    {
        fadeManager.Show();
        playerController.Initialise();
        SceneController.Instance.LoadNewScene(SaveManager.Instance.SaveData.doesDataExist
            ? SaveManager.Instance.SaveData.currentSceneIdx
            : 1);
    }
    
    public void TogglePlayer(bool toggle)
    {
        playerController?.ToggleAllInputs(toggle);
    }

    public void MovePlayer(LoadZoneData data)
    {
        if (!playerController.HasInitialised) return;
        bool canMove = playerController.CanMove;
        bool canRotateCam = playerController.CanRotate;

        if (canMove)
            playerController.ToggleMovement(false);
        if (canRotateCam)
            playerController.ToggleCameraInput(false);

        playerController.transform.position = data.newPosition;
        playerController.RotatePlayer(data.newRotation);
        playerController.ForceRotateCamera(data.cameraRotation);

        if (canMove)
            playerController.ToggleMovement(true);
        if (canRotateCam)
            playerController.ToggleCameraInput(true);
    }
}