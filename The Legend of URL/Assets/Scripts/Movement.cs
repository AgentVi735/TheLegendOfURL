using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private CharacterController controller;

    [SerializeField] private Transform characterTrans;
    
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionAsset;
    private InputAction movementInput;
    [SerializeField] private string movementInputPath;
    private InputAction runInput;
    [SerializeField] private string runInputPath;
    private bool isRunning;
    private Vector2 moveAmount;
    [SerializeField] private float maxTurnDiff;
    private bool hasToTurn;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float runTurnSpeed;
    
    [Header("Options")]
    [SerializeField] private float speed;
    [SerializeField] private float runModifier;

    private void Awake()
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
        moveAmount = movementInput.ReadValue<Vector2>();
        if (moveAmount == Vector2.zero) return;
        
        Vector3 movePos = new(moveAmount.x, 0, moveAmount.y);
        
        float deltaAngle = Vector3.Angle(characterTrans.forward, movePos);
        Vector3 rotationAxis = Vector3.Cross(characterTrans.forward, movePos);
        Quaternion deltaRotation = Quaternion.AngleAxis(deltaAngle, rotationAxis);
        characterTrans.rotation = Quaternion.Lerp(characterTrans.rotation, characterTrans.rotation * deltaRotation,
            (isRunning ? runTurnSpeed : turnSpeed) * Time.deltaTime);
        
        if (!hasToTurn)
            controller.Move(characterTrans.forward * (speed * Time.deltaTime));
    }

    private void OnRunEntered(InputAction.CallbackContext ctx)
    {
        if (isRunning) return;
        isRunning = true;
        speed += runModifier;
    }

    private void OnRunCancelled(InputAction.CallbackContext ctx)
    {
        if (!isRunning) return;
        isRunning = false;
        speed -= runModifier;
    }
}
