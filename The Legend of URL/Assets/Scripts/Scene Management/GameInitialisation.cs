using UnityEngine;

public class GameInitialisation : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Vector3 playerSpawnPos;
    [SerializeField] private FadeManager fadeManager;

    private void Start()
    {
        fadeManager.Show();
        SceneController.Instance.LoadNewScene(1);
    }
}