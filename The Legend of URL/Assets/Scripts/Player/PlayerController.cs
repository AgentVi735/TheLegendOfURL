using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class PlayerController : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAttackManager attackManager;
    public PlayerHUDController HudController => _hudController;
    [SerializeField] private PlayerHUDController _hudController;
    public CharacterController CharacterController => _characterController;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform characterTransform;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineOrbitalFollow cinemachineOrbitalFollow;
    [SerializeField] private CinemachineInputAxisController cinemachineInputController;

    [Header("Stats")]
    [SerializeField] private short maxHealth;
    private short health;
    private Collider[] interactColliders;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionAsset;
    private InputAction interactInput;
    [SerializeField] private string interactInputPath;
    
    [Header("Options")]
    public bool CanRotate { get; private set; }
    public bool CanCameraFollow { get; private set; }
    public bool CanMove => movement.CanMove;
    public bool CanRun => movement.CanRun;
    public bool CanJump => movement.CanJump;
    public bool CanPause => movement.CanPause;
    public bool CanAttack => attackManager.CanAttack;
    public bool CanLockOn => attackManager.CanLockOn;
    public bool CanBeHit { get; private set; }
    public bool CanInteract { get; private set; }
    public bool HasInitialised { get; private set; }
    [SerializeField] private float interactRadius;
    [SerializeField] private LayerMask interactLayerMask;
    [SerializeField] private int maxInteractColliders = 1;
    [SerializeField] private string npcTag;
    
#if UNITY_EDITOR
    [Header("Editor Options")]
    [SerializeField] private int targetFrameRateEditor = -1;
#endif

    public void Initialise()
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
        interactColliders = new Collider[maxInteractColliders];
        SceneController.Instance.DialogueController.OnOpenStory.AddListener(() => { ToggleAllInputs(false);});
        SceneController.Instance.DialogueController.OnCloseStory.AddListener(() => { ToggleAllInputs(true);});
        
        interactInput = inputActionAsset.FindAction(interactInputPath);
        if (interactInput == null)
        {
            Debug.LogError($"InteractInputPath is invalid on object {gameObject.name}");
            gameObject.SetActive(false);
            return;
        }

        interactInput.started += OnInteractPressed;
        
        movement.Initialise();
        attackManager.Initialise();

        _hudController.Initialise(maxHealth);

        HasInitialised = true;
    }

    private void OnDestroy()
    {
        if (interactInput != null)
            interactInput.started -= OnInteractPressed;

        if (SceneController.Instance?.DialogueController == null) return;
        SceneController.Instance.DialogueController.OnOpenStory.RemoveListener(() => { ToggleAllInputs(false);});
        SceneController.Instance.DialogueController.OnCloseStory.RemoveListener(() => { ToggleAllInputs(true);});
    }

    public void ToggleCameraInput(bool toggle)
    {
        CanRotate = toggle;
        cinemachineInputController.enabled = toggle;
    }
    public void ToggleCameraFollow(bool toggle)
    {
        CanCameraFollow = toggle;
        cinemachineCamera.enabled = toggle;
    }
    public void TogglePause(bool toggle) => movement.TogglePause(toggle);
    public void ToggleMovement(bool toggle) => movement.ToggleMovement(toggle);
    public void ToggleGravity(bool toggle) => movement.ToggleGravity(toggle);
    public void ToggleRun(bool toggle) => movement.ToggleRun(toggle);
    public void ToggleJump(bool toggle) => movement.ToggleJump(toggle);
    public void ToggleAttack(bool toggle) => attackManager.ToggleAttack(toggle);
    public void ToggleLockOn(bool toggle) => attackManager.ToggleLockOn(toggle);
    public void ToggleReceiveDamage(bool toggle) => CanBeHit = toggle;
    public void ToggleCursor(bool toggle)
    {
        Cursor.visible = toggle;
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Confined;
    }
    public void ToggleInteract(bool toggle) => CanInteract = toggle;

    public void OnHit(short receivedDamage)
    {
        if (!CanBeHit) return;
        health -= receivedDamage;
        _hudController.UpdateHealthBar(health);
        if (health < 0)
            OnDeath();
    }

    private void OnDeath()
    {
        ToggleAllInputs(false);
        TogglePause(true);
        print("Death :3");
    }

    private void OnInteractPressed(InputAction.CallbackContext ctx)
    {
        if (!CanInteract) return;
        
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, interactRadius, interactColliders,
            interactLayerMask);
        if (hitCount <= 0) return;
        int idx = -1;
        foreach (Collider collider in interactColliders)
        {
            idx++;
            if (collider == null)
                continue;
            Debug.Log($"Index: {idx} | Collider: {collider.name} | Distance: {Vector3.Distance(transform.position, collider.transform.position)}");
            if (collider.CompareTag(npcTag))
                InteractWithNpc(collider.gameObject);
        }
    }

    public void ToggleAllInputs(bool toggle)
    {
        TogglePause(toggle);
        ToggleMovement(toggle);
        ToggleGravity(toggle);
        ToggleRun(toggle);
        ToggleJump(toggle);
        ToggleAttack(toggle);
        ToggleLockOn(toggle);
        ToggleCameraInput(toggle);
        ToggleCameraFollow(toggle);
        ToggleReceiveDamage(toggle);
        ToggleInteract(toggle);
        ToggleCursor(!toggle);
    }
    
    private void InteractWithNpc(GameObject npcObject)
    {
        NpcController npcController = npcObject.GetComponent<NpcController>();
        npcController.StartStory();
    }

    public short EnemyGetDamage() => attackManager.damage;

    public void ForceRotateCamera(float rotation)
    {
        cinemachineCamera.OnTargetObjectWarped(characterTransform, characterTransform.position);
        cinemachineCamera.ForceCameraPosition(characterTransform.position + -characterTransform.forward * 8,
            Quaternion.Euler(cinemachineOrbitalFollow.VerticalAxis.Center, rotation, 0));
    }

    public void RotatePlayer(Vector3 rotation) => movement.RotatePlayer(rotation);

    public void LoadSaveData()
    {
        Debug.Log("Loading player save");
        health = SaveManager.Instance.SaveData.health;
        _hudController.UpdateHealthBar(health);
        UpdateSensitivity();
        movement.LoadSaveData();
    }
    
    public void SaveData()
    {
        if (!gameObject.activeSelf) return;
        SaveManager.Instance.SaveData.health = health;
        movement.SaveData();
    }

    public void OnDeviceChange(ControlScheme newScheme)
    {
        UpdateSensitivity(newScheme);
    }

    private void UpdateSensitivity()
    {
        Vector2 sensitivity = SceneController.ControlScheme switch
        {
            ControlScheme.Gamepad => SaveManager.Instance.SaveData.controllerSensitivity,
            _ => SaveManager.Instance.SaveData.mouseSensitivity
        };

        cinemachineInputController.Controllers[0].Input.Gain = sensitivity.x;
        cinemachineInputController.Controllers[1].Input.Gain = -sensitivity.y;
    }

    private void UpdateSensitivity(ControlScheme scheme)
    {
        Vector2 sensitivity = SceneController.ControlScheme switch
        {
            ControlScheme.Gamepad => SaveManager.Instance.SaveData.controllerSensitivity,
            _ => SaveManager.Instance.SaveData.mouseSensitivity
        };

        cinemachineInputController.Controllers[0].Input.Gain = sensitivity.x;
        cinemachineInputController.Controllers[1].Input.Gain = -sensitivity.y;
    }
}