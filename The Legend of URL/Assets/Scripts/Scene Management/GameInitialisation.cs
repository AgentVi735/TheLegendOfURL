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
        sceneController.DialogueController.Initialise();
        SceneController.Instance.LoadNewScene(SaveManager.Instance.SaveData.doesDataExist
            ? SaveManager.Instance.SaveData.currentSceneIdx
            : sceneController.SceneHolder.StartGameSceneIdx );
    }
    
    public void TogglePlayer(bool toggle)
    {
        playerController?.ToggleAllInputs(toggle);
    }

    public void MovePlayer(LoadZoneData data)
    {
        Debug.Log($"Moving player with pos: {data.newPosition}");
        if (!playerController.HasInitialised) return;
        
        bool wasActive = playerController.isActiveAndEnabled;
        playerController.gameObject.SetActive(false);
        
        bool canMove = playerController.CanMove;
        bool canRotateCam = playerController.CanRotate;

        if (canMove)
        {
            playerController.ToggleMovement(false);
            playerController.ToggleGravity(false);
        }
        if (canRotateCam)
            playerController.ToggleCameraInput(false);

        playerController.transform.position = data.newPosition;
        playerController.RotatePlayer(data.newRotation);
        playerController.ForceRotateCamera(data.cameraRotation);

        if (canMove)
        {
            playerController.ToggleMovement(true);
            playerController.ToggleGravity(true);
        }
        if (canRotateCam)
            playerController.ToggleCameraInput(true);
        
        if (wasActive)
            playerController.gameObject.SetActive(true);
        
        Debug.Log($"Player is now at {playerController.transform.position}");
    }
}