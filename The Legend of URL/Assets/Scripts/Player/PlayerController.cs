using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private Camera _camera;
    public AudioPlayer AudioPlayer => _audioPlayer;
    [SerializeField] private AudioPlayer _audioPlayer;
    public Animator Animator => _animator;
    [SerializeField] private Animator _animator;

    [Header("Stats")]
    [SerializeField] private short maxHealth;
    private short health;
    private Collider[] interactColliders;
    [SerializeField] private short lifestealHealth;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionAsset;
    private InputAction interactInput;
    [SerializeField] private string interactInputPath;
    
    [Header("Options")]
    public bool CanRotate { get; private set; }
    public bool CanMove => movement.CanMove;
    private bool CanBeHit { get; set; }
    private bool CanInteract { get; set; }
    public bool HasInitialised { get; private set; }
    [SerializeField] private float interactRadius;
    [SerializeField] private LayerMask interactLayerMask;
    [SerializeField] private int maxInteractColliders = 1;
    [SerializeField] private string npcTag;
    private NpcController closestNpc;
    
#if UNITY_EDITOR
    [Header("Editor Options")]
    [SerializeField] private int targetFrameRateEditor = -1;
#endif

#if UNITY_EDITOR
    private void EditorChangeFrameRate()
    {
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
    }
#endif
    
    public void Initialise()
    {
#if UNITY_EDITOR
        EditorChangeFrameRate();
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

    public void ShouldResetSave()
    {
        movement.Pause();
    }

    public void ToggleCameraInput(bool toggle)
    {
        CanRotate = toggle;
        cinemachineInputController.enabled = toggle;
    }

    private void ToggleCameraFollow(bool toggle) => cinemachineCamera.enabled = toggle;
    private void TogglePause(bool toggle) => movement.TogglePause(toggle);
    public void ToggleMovement(bool toggle) => movement.ToggleMovement(toggle);
    public void ToggleGravity(bool toggle) => movement.ToggleGravity(toggle);
    private void ToggleRun(bool toggle) => movement.ToggleRun(toggle);
    private void ToggleJump(bool toggle) => movement.ToggleJump(toggle);
    public void ToggleAttack(bool toggle) => attackManager.ToggleAttack(toggle);
    private void ToggleLockOn(bool toggle) => attackManager.ToggleLockOn(toggle);
    public void ToggleReceiveDamage(bool toggle) => CanBeHit = toggle;
    public static void ToggleCursor(bool toggle)
    {
        Cursor.visible = toggle;
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Confined;
    }
    private void ToggleInteract(bool toggle) => CanInteract = toggle;

    public void ResetJump() => movement.ResetJump();

    public void OnHit(short receivedDamage)
    {
        if (!CanBeHit) return;
        health -= receivedDamage;
        _hudController.UpdateHealthBar(health);
        _audioPlayer.PlayClip(0);
        if (health < 0)
            OnDeath();
    }

    private void OnDeath()
    {
        ShouldResetSave();
        SaveManager.Instance.ResetSave();
    }

    public void OnEnemyKill()
    {
        health += lifestealHealth;
        if (health > 100)
            health = 100;
        _hudController.UpdateHealthBar(health);
    }

    private void Update()
    {
        if (CanMove && CanRotate && CanInteract)
            CheckForInteractions();
        else
            _hudController.ToggleInteractButton(false);
    }

    private void CheckForInteractions()
    {
        if (!CanInteract) return;
        
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, interactRadius, interactColliders,
            interactLayerMask);
        if (hitCount <= 0)
        {
            closestNpc = null;
            _hudController.ToggleInteractButton(false);
            return;
        }

        Collider closestCollider = null;
        
        foreach (Collider collider in interactColliders)
        {
            if (collider == null)
                continue;
            if (collider.CompareTag(npcTag))
                closestCollider = collider;
        }

        closestNpc = closestCollider?.GetComponent<NpcController>();

        if (closestCollider?.transform == null)
        {
            _hudController.ToggleInteractButton(false);
            return;
        }

        if (closestNpc == null)
            return;

        Vector3 npcPos = closestNpc.InteractButtonPosition + closestNpc.transform.position;
        Vector2 buttonPos = _camera.WorldToScreenPoint(npcPos);
        _hudController.SetInteractButtonPosition(buttonPos);
        _hudController.ToggleInteractButton(true);
    }

    public void UpdateHealthFromSave() => HudController.UpdateHealthBar(SaveManager.Instance.SaveData.health);
    
    private void OnInteractPressed(InputAction.CallbackContext ctx)
    {
        if (!CanInteract) return;
        
        if (closestNpc == null)
        {
            CheckForInteractions();
            if (closestNpc == null)
                return;
        }
        
        closestNpc.StartStory();
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
        _hudController.ToggleBossHealthBar(false);
        _hudController.SetBossMaxHealth(0);
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
        UpdateSensitivity();
        _hudController.SetInteractButtonSprite(newScheme);
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
}