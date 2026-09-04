using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform characterTrans;
    [SerializeField] private Transform camTrans;
    
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionAsset;
    private InputAction movementInput;
    [SerializeField] private string movementInputPath;
    private InputAction runInput;
    [SerializeField] private string runInputPath;
    private bool isRunning;
    [SerializeField] private float maxTurnDiff;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float runTurnSpeed;
    
    [Header("Options")]
    [SerializeField] private float speed;
    [SerializeField] private float runModifier;
    [SerializeField] private float gravitySpeed;
    public bool CanMove;
    public bool CanRun;

    public void Initialise()
    {
        movementInput = inputActionAsset.FindAction(movementInputPath);
        if (movementInput == null)
        {
            Debug.LogError($"MovementInputPath is invalid on object {gameObject.name}");
            gameObject.SetActive(false);
            return;
        }

        runInput = inputActionAsset.FindAction(runInputPath);
        if (runInput == null)
        {
            Debug.LogError($"RunInputPath is invalid on object {gameObject.name}");
            gameObject.SetActive(false);
            return;
        }

        runInput.started += OnRunEntered;
        runInput.canceled += OnRunCancelled;
    }

    private void OnDestroy() => DisposeActions();

    private void DisposeActions()
    {
        if (runInput == null) return;
        runInput.started -= OnRunEntered;
        runInput.canceled -= OnRunCancelled;
    }

    private void Update()
    {
        Vector3 velocity = Vector3.zero;
        if (controller.isGrounded)
        {
            if (velocity.y < -2f)
                velocity.y = -2f;
        }
        
        velocity.y += gravitySpeed * Time.deltaTime;

        controller.Move(-characterTrans.up * velocity.y);
        
        if (!CanMove) return;
        
        Vector2 moveAmount = movementInput.ReadValue<Vector2>();
        if (moveAmount == Vector2.zero) return;
        
        float deltaRot = -camTrans.rotation.eulerAngles.y;
        Quaternion rotation = Quaternion.AngleAxis(deltaRot, Vector3.forward);
        moveAmount = rotation * moveAmount;
        
        Vector3 movePos = new(moveAmount.x, 0, moveAmount.y);
        
        float deltaAngle = Vector3.Angle(characterTrans.forward, movePos);
        Vector3 rotationAxis = Vector3.Cross(characterTrans.forward, movePos);
        Quaternion deltaRotation = Quaternion.AngleAxis(deltaAngle, rotationAxis);
        characterTrans.rotation = Quaternion.Lerp(characterTrans.rotation, characterTrans.rotation * deltaRotation,
            (isRunning ? runTurnSpeed : turnSpeed) * Time.deltaTime);

        controller.Move(characterTrans.forward * (speed * Time.deltaTime));
    }

    private void OnRunEntered(InputAction.CallbackContext ctx)
    {
        if (isRunning || !CanRun) return;
        isRunning = true;
        speed += runModifier;
    }

    private void OnRunCancelled(InputAction.CallbackContext ctx)
    {
        if (!isRunning) return;
        isRunning = false;
        speed -= runModifier;
    }
    
    public void ToggleMovement(bool toggle)
    {
        CanMove = toggle;
        if (toggle)
            movementInput.Enable();
        else
            movementInput.Disable();
    }

    public void ToggleRun(bool toggle)
    {
        CanRun = toggle;
        if (toggle)
            runInput.Enable();
        else
            runInput.Disable();
    }
}
