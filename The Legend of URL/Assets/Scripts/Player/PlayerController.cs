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

#if UNITY_EDITOR
    [Header("Editor Options")]
    [SerializeField] private int targetFrameRateEditor = -1;
#endif

    private void Awake()
    {
        // TODO: PUT THIS INTO A METHOD PLS
#if UNITY_EDITOR
        if (targetFrameRateEditor > 0)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRateEditor;
        }
        else
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
        }
#endif        
        health = maxHealth;
        
        movement.Initialise();
        attackManager.Initialise();
        
        ToggleAllInputs(true);
    }

    private void ToggleCameraInput(bool toggle) => cinemachineInputController.enabled = toggle;
    private void ToggleCameraFollow(bool toggle) => cinemachineCamera.enabled = toggle;
    public void ToggleMovement(bool toggle) => movement.ToggleMovement(toggle);
    public void ToggleRun(bool toggle) => movement.ToggleRun(toggle);
    public void ToggleJump(bool toggle) => movement.ToggleJump(toggle);
    public void ToggleAttack(bool toggle) => attackManager.ToggleAttack(toggle);
    public void ToggleLockOn(bool toggle) => attackManager.ToggleLockOn(toggle);

    public void OnHit(short receivedDamage)
    {
        health -= receivedDamage;
        if (health < 0)
            OnDeath();
    }

    private void OnDeath()
    {
        ToggleAllInputs(false);
        print("Death :3");
    }

    private void ToggleAllInputs(bool toggle)
    {
        ToggleMovement(toggle);
        ToggleRun(toggle);
        ToggleJump(toggle);
        ToggleAttack(toggle);
        ToggleLockOn(toggle);
    }

    public short EnemyGetDamage() => attackManager.damage;
}