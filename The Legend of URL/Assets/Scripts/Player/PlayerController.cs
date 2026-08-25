using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineInputAxisController cinemachineInputController;

    [Header("Stats")]
    [SerializeField] private short maxHealth;
    private short health;
    [SerializeField] private short damage;

    private void Awake()
    {
        health = maxHealth;
        
        movement.Initialise();

        movement.ToggleMovement(true);
        movement.ToggleRun(true);
    }

    private void ToggleCameraInput(bool toggle) => cinemachineInputController.enabled = toggle;
    private void ToggleCameraFollow(bool toggle) => cinemachineCamera.enabled = toggle;

    private void OnHit(short receivedDamage)
    {
        health -= receivedDamage;
        if (health < 0)
            OnDeath();
    }

    private void OnDeath()
    {
        movement.ToggleMovement(false);
        ToggleCameraInput(false);
        print("Death :3");
    }
}