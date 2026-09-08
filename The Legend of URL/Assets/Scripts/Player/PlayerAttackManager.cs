using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackManager : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private CinemachineCamera camera;
    [SerializeField] private PlayerController controller;
    [SerializeField] private Transform characterTransform;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private BoxCollider swordCollider;
    [SerializeField] private Animator swordAnimator; // TODO: TEMPORARY, THIS WILL GET REPLACED WITH PLAYER ANIMATOR
    [SerializeField] private string swordAnimationPath; // TODO: TEMPORARY

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionAsset;
    private InputAction attackInput;
    [SerializeField] private string attackInputPath;
    private InputAction lockOnInput;
    [SerializeField] private string lockOnInputPath;

    [Header("Stats")]
    public short damage;

    [Header("Options")]
    public bool CanAttack;
    private bool isAttacking;
    [SerializeField] private float attackTime;
    private WaitForSeconds waitAttackTime;
    public bool CanLockOn;
    [SerializeField] private float lockOnRadius;
    [SerializeField] private LayerMask lockOnLayerMask;
    private bool isLockedOn;
    private EnemyController lockedEnemy;

    public PlayerAttackManager(PlayerController controller)
    {
        this.controller = controller;
    }

    public void Initialise()
    {
        swordCollider.enabled = false;
        attackInput = inputActionAsset.FindAction(attackInputPath);
        if (attackInput == null)
        {
            Debug.LogError($"AttackInputPath is invalid on object {gameObject.name}");
            gameObject.SetActive(false);
            return;
        }
        lockOnInput = inputActionAsset.FindAction(lockOnInputPath);
        if (lockOnInput == null)
        {
            Debug.LogError($"LockOnInputPath is invalid on object {gameObject.name}");
            gameObject.SetActive(false);
            return;
        }
        
        attackInput.started += OnAttackInput;
        lockOnInput.started += OnLockOnInput;

        waitAttackTime = new WaitForSeconds(attackTime);
    }

    private void OnDestroy()
    {
        if (attackInput != null)
            attackInput.started -= OnAttackInput;
        if (lockOnInput != null)
            lockOnInput.started -= OnLockOnInput;
    }
    
    private void OnAttackInput(InputAction.CallbackContext ctx)
    {
        if (!CanAttack || isAttacking) return;
        StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        controller.ToggleMovement(false);
        swordCollider.enabled = true;
        swordAnimator.SetTrigger(swordAnimationPath);

        yield return waitAttackTime;

        AttackFinish();
        controller.ToggleMovement(true);
    }

    private void AttackFinish()
    {
        swordCollider.enabled = false;
        isAttacking = false;
    }
    
    private void OnLockOnInput(InputAction.CallbackContext ctx)
    {
        if (!CanLockOn) return;
        if (isLockedOn)
            DisableLockOn();
        else
            EnableLockOn();
    }

    private void EnableLockOn()
    {
        Collider[] foundColliders = new Collider[10];
        Physics.OverlapSphereNonAlloc(transform.position, lockOnRadius, foundColliders, lockOnLayerMask);

        Vector3 camForward = camera.transform.forward;
        Collider closestEnemy = null;
        float closestEnemyAngle = 0;
        foreach (Collider collider in foundColliders)
        {
            if (collider == null || !collider.CompareTag("Enemy")) continue;

            camForward.y = collider.transform.position.y;
            float angle = Vector3.Angle(camForward, collider.transform.position);
            if (angle < closestEnemyAngle) continue;
            closestEnemy = collider;
            closestEnemyAngle = angle;
        }

        if (closestEnemy == null) return;
        lockedEnemy = closestEnemy.GetComponent<EnemyController>();
        
        isLockedOn = true;
    }

    private void DisableLockOn()
    {
        isLockedOn = false;
        lockedEnemy = null;
        cameraTarget.position = characterTransform.position;
    }

    public void ToggleAttack(bool toggle)
    {
        CanAttack = toggle;
        if (toggle)
            attackInput.Enable();
        else
        {
            attackInput.Disable();
            AttackFinish();
        }
    }

    public void ToggleLockOn(bool toggle)
    {
        CanLockOn = toggle;
        if (toggle)
            lockOnInput.Enable();
        else
        {
            lockOnInput.Disable();
            if (isLockedOn)
                DisableLockOn();
        }
    }

    private void Update()
    {
        if (!isLockedOn) return;
        if (lockedEnemy == null)
        {
            DisableLockOn();
            return;
        }
        cameraTarget.position = Vector3.Lerp(characterTransform.position, lockedEnemy.transform.position, 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lockOnRadius);
        if (lockedEnemy == null) return;
        Gizmos.color = Color.blueViolet;
        Gizmos.DrawLine(camera.transform.position, lockedEnemy.transform.position);
    }
}