using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackManager : MonoBehaviour
{
    private static readonly int attackAnim = Animator.StringToHash("attack");

    [Header("Player References")]
    [SerializeField] private CinemachineCamera camera;
    [SerializeField] private PlayerController controller;
    [SerializeField] private Transform characterTransform;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private BoxCollider swordCollider;

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
    public bool CanLockOn;
    [SerializeField] private float lockOnRadius;
    [SerializeField] private LayerMask lockOnLayerMask;
    [SerializeField] private LayerMask lockOnCheckLayerMask;
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
        if (!CanAttack || isAttacking || !controller.hasUnlockedCombat) return;
        
        isAttacking = true;
        controller.ToggleMovement(false);
        swordCollider.enabled = true;
        controller.Animator.SetTrigger(attackAnim);
        controller.AudioPlayer.PlayClip(1);
    }

    public void AttackFinish()
    {
        swordCollider.enabled = false;
        isAttacking = false;
        if (controller.CanRotate)
            controller.ToggleMovement(true);
    }
    
    private void OnLockOnInput(InputAction.CallbackContext ctx)
    {
        if (!CanLockOn || !controller.hasUnlockedCombat) return;
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
        Vector3 enemyPos = lockedEnemy.transform.TransformPoint(lockedEnemy.LockOnArrowOffset);
        Vector2 arrowPos = controller.Camera.WorldToScreenPoint(enemyPos);
        controller.HudController.SetLockOnArrowPosition(arrowPos);
        controller.HudController.ToggleLockOnArrow(true);
    }

    private void DisableLockOn()
    {
        isLockedOn = false;
        lockedEnemy = null;
        cameraTarget.position = characterTransform.position;
        controller.HudController.ToggleLockOnArrow(false);
    }

    public void ToggleAttack(bool toggle)
    {
        CanAttack = toggle;
        if (toggle)
            attackInput.Enable();
        else
        {
            attackInput.Disable();
            if (isAttacking)
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
        if (lockedEnemy == null ||
            Vector3.Distance(characterTransform.position, lockedEnemy.transform.position) > lockOnRadius)
        {
            DisableLockOn();
            return;
        }

        cameraTarget.position = Vector3.Lerp(characterTransform.position, lockedEnemy.transform.position, 0.5f);
        
        Vector3 enemyPos = lockedEnemy.transform.TransformPoint(lockedEnemy.LockOnArrowOffset);
        Vector2 arrowPos = controller.Camera.WorldToScreenPoint(enemyPos);
        controller.HudController.SetLockOnArrowPosition(arrowPos);
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