using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
    private InputAction pauseInput;
    [SerializeField] private string pauseInputPath;
    private InputAction movementInput;
    [SerializeField] private string movementInputPath;
    private InputAction runInput;
    [SerializeField] private string runInputPath;
    private bool isRunning;
    private InputAction jumpInput;
    [SerializeField] private string jumpInputPath;
    private Vector3 jumpVelocity;
    [SerializeField] private float maxTurnDiff;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float runTurnSpeed;
    
    [Header("Options")]
    [SerializeField] private float speed;
    [SerializeField] private float strollInputMax;
    [SerializeField] private float strollSpeed;
    [SerializeField] private float runModifier;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private Vector3 baseJumpVelocity;
    [SerializeField] private float gravitySpeed;
    public bool CanPause;
    public bool CanMove;
    public bool CanRun;
    public bool CanJump;
    private bool isOnGround;
#if UNITY_EDITOR
    public bool disableKeyboard;
#endif

    public void Initialise()
    {
#if UNITY_EDITOR
        if (disableKeyboard)
        {
            foreach (InputDevice inputDevice in InputSystem.devices)
            {
                if (inputDevice is Keyboard or Mouse)
                    InputSystem.DisableDevice(inputDevice);
                else
                    InputSystem.EnableDevice(inputDevice);
            }
        }
        else
            foreach (InputDevice inputDevice in InputSystem.devices)
                InputSystem.EnableDevice(inputDevice);
#endif

        pauseInput = inputActionAsset.FindAction(pauseInputPath);
        if (pauseInput == null)
        {
            Debug.LogError($"PauseInputPath is invalid on object {gameObject.name}");
            gameObject.SetActive(false);
            return;
        }

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

        jumpInput = inputActionAsset.FindAction(jumpInputPath);
        if (runInput == null)
        {
            Debug.LogError($"JumpInputPath is invalid on object {gameObject.name}");
            gameObject.SetActive(false);
            return;
        }

        pauseInput.started += OnPausePressed;
        runInput.started += OnRunEntered;
        runInput.canceled += OnRunCancelled;
        jumpInput.started += OnJumpPressed;
    }

    private void OnDestroy() => DisposeActions();

    private void DisposeActions()
    {
        if (pauseInput != null)
            pauseInput.started -= OnPausePressed;
        if (runInput != null)
        {
            runInput.started -= OnRunEntered;
            runInput.canceled -= OnRunCancelled;
        }
        if (jumpInput != null)
            jumpInput.started -= OnJumpPressed;
    }

    private void Update()
    {
        isOnGround = Physics.Raycast(transform.position, -transform.up, out RaycastHit hit,
            0.2f);
        
        Vector3 velocity = Vector3.zero;
        if (isOnGround)
        {
            if (velocity.y < -2f)
                velocity.y = -2f;
        }
        
        velocity.y += gravitySpeed * Time.deltaTime;

        Vector3 movePos = Vector3.zero; 
        movePos += -characterTrans.up * velocity.y;
        if (jumpVelocity.y > 0)
        {
            Vector3 extraVelocity = jumpVelocity * (jumpSpeed * Time.deltaTime);
            movePos += extraVelocity;
            jumpVelocity -= extraVelocity;
            if (jumpVelocity.y < 0.1f || (jumpVelocity.y < baseJumpVelocity.y / 3 && isOnGround))
                jumpVelocity.y = 0;
        }

        controller.Move(movePos);
        
        if (!CanMove) return;
        
        Vector2 moveAmount = movementInput.ReadValue<Vector2>();
        if (moveAmount == Vector2.zero) return;
        
        float deltaRot = -camTrans.rotation.eulerAngles.y;
        Quaternion rotation = Quaternion.AngleAxis(deltaRot, Vector3.forward);
        moveAmount = rotation * moveAmount;
        
        movePos = new Vector3(moveAmount.x, 0, moveAmount.y);
        
        float deltaAngle = Vector3.Angle(characterTrans.forward, movePos);
        Vector3 rotationAxis = Vector3.Cross(characterTrans.forward, movePos);
        Quaternion deltaRotation = Quaternion.AngleAxis(deltaAngle, rotationAxis);
        characterTrans.rotation = Quaternion.Lerp(characterTrans.rotation, characterTrans.rotation * deltaRotation,
            (isRunning ? runTurnSpeed : turnSpeed) * Time.deltaTime);

        float amtX = Math.Abs(moveAmount.x);
        float amtY = Math.Abs(moveAmount.y);
        
        float highestInputAmt = Math.Max(amtX, amtY);
        float currentSpeed = speed;
        if (!isRunning && highestInputAmt < strollInputMax)
            currentSpeed = strollSpeed;
        
        controller.Move(characterTrans.forward * (currentSpeed * Time.deltaTime));
    }

    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        if (!CanPause) return;
        
        Quit();
    }
    
    private static void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
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

    private void OnJumpPressed(InputAction.CallbackContext ctx)
    {
        if (!isOnGround || jumpVelocity.y > 0) return;
        jumpVelocity += baseJumpVelocity;
    }
    
    public void TogglePause(bool toggle)
    {
        CanPause = toggle;
        if (toggle)
            pauseInput.Enable();
        else
            pauseInput.Disable();
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

    public void ToggleJump(bool toggle)
    {
        CanJump = toggle;
        if (toggle)
            jumpInput.Enable();
        else
            jumpInput.Disable();
    }
}
