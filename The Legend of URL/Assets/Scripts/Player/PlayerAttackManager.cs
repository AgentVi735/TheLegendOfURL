using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackManager : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerController controller;
    [SerializeField] private BoxCollider swordCollider;
    [SerializeField] private Animator swordAnimator; // TODO: TEMPORARY, THIS WILL GET REPLACED WITH PLAYER ANIMATOR
    [SerializeField] private string swordAnimationPath; // TODO: TEMPORARY

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionAsset;
    private InputAction attackInput;
    [SerializeField] private string attackInputPath;

    [Header("Stats")]
    public short damage;

    [Header("Options")]
    public bool CanAttack;
    private bool isAttacking;
    [SerializeField] private float attackTime;
    private WaitForSeconds waitAttackTime;

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
        
        attackInput.started += OnAttackInput;

        waitAttackTime = new WaitForSeconds(attackTime);
    }

    private void OnDestroy()
    {
        if (attackInput != null)
            attackInput.started -= OnAttackInput;
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
}