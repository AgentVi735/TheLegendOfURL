using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAttackManager attackManager;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineInputAxisController cinemachineInputController;

    [Header("Stats")]
    [SerializeField] private short maxHealth;
    private short health;

    private void Awake()
    {
        // TODO: PUT THIS INTO A METHOD PLS
#if UNITY_EDITOR
        // Application.targetFrameRate = 30;
#endif        
        health = maxHealth;
        
        movement.Initialise();
        attackManager.Initialise();

        ToggleMovement(true);
        ToggleRun(true);
        ToggleAttack(true);
    }

    private void ToggleCameraInput(bool toggle) => cinemachineInputController.enabled = toggle;
    private void ToggleCameraFollow(bool toggle) => cinemachineCamera.enabled = toggle;
    public void ToggleMovement(bool toggle) => movement.ToggleMovement(toggle);
    public void ToggleRun(bool toggle) => movement.ToggleRun(toggle);
    public void ToggleAttack(bool toggle) => attackManager.ToggleAttack(toggle);

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

    public short EnemyGetDamage() => attackManager.damage;
}