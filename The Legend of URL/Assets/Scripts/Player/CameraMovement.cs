using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Transform camTrans;
    [SerializeField] private Transform xMovementTransform;
    [SerializeField] private Transform yMovementTransform;
    [SerializeField] private Transform characterTrans;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionAsset;
    private InputAction moveInput;
    [SerializeField] private string movementInputPath;
    
    [Header("Options")]
    [SerializeField] private float xSensitivity;
    [SerializeField] private float ySensitivity;
    [SerializeField] private Vector2 turnYClamp;
    
    private void Awake()
    {
        moveInput = inputActionAsset.FindAction(movementInputPath);
        if (moveInput != null) return;
        Debug.LogError($"MovementInputPath is invalid on object {gameObject.name}");
        gameObject.SetActive(false);
    }
    
    private void Update()
    {
        Vector2 moveAmount = moveInput.ReadValue<Vector2>();
        if (moveAmount == Vector2.zero) return;

        camTrans.RotateAround(characterTrans.position, camTrans.up, moveAmount.x * xSensitivity);
        camTrans.RotateAround(characterTrans.position, camTrans.right, -moveAmount.y * ySensitivity);
        
        // print($"{characterTrans.position} | {camTrans.up} | {moveAmount.x * xSensitivity}");
        // print($"{characterTrans.position} | {camTrans.right} | {-moveAmount.y * ySensitivity");
        
        // if (camTrans.localPosition.y > turnYClamp.x)
        //     camTrans.localPosition = new Vector3(camTrans.localPosition.x, turnYClamp.x, camTrans.localPosition.z);
        // else if (camTrans.localPosition.y < turnYClamp.y)
        //     camTrans.localPosition = new Vector3(camTrans.localPosition.x, turnYClamp.y, camTrans.localPosition.z);
    }
}