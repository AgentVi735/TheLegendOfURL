using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;
    private static bool s_hasInitialised;
    private static bool s_hasStartedInitialisation;

    public static UnityAction<Scene, LoadSceneMode> s_OnSceneLoaded;
    public static UnityAction<Scene> s_OnSceneUnloaded;

    public SceneHolder SceneHolder => _sceneHolder;
    [SerializeField] private SceneHolder _sceneHolder;
    [SerializeField] private GameInitialisation _initialisation;
    public FadeManager FadeManager => _fadeManager;
    [SerializeField] private FadeManager _fadeManager;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private SaveManager _saveManager;
    [SerializeField] private bool _isMainMenuController;

    private static string s_currentSceneName;
    private static string s_initSceneName;

    private static LoadZoneData s_currentZoneData;

    private static SceneHandle s_initScene;
    private static SceneHandle s_currentScene;

    private static InputDevice s_lastDevice;
    public static ControlScheme ControlScheme;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        if (_isMainMenuController)
            return;
        
        StartupCheck();
    }

    private void StartupCheck()
    {
        if (_initialisation == null)
        {
            Initialise();
            if (gameObject != null)
                Destroy(gameObject);
            return;
        }

        switch (s_hasInitialised)
        {
            case false:
                Initialise();
                break;
            case true:
                LoadNewScene(SaveManager.Instance.SaveData.doesDataExist
                    ? SaveManager.Instance.SaveData.currentSceneIdx
                    : SceneHolder.StartGameSceneIdx);
                _playerController.HudController.gameObject.SetActive(true);
                _playerController.gameObject.SetActive(true);
                _initialisation.gameObject.SetActive(true);
                break;
        }
        
        if (SaveManager.Instance == null)
            _saveManager.Initialise();
        if (_saveManager.SaveData.doesDataExist)
            _playerController.LoadSaveData();

        if (Instance != null) return;
        Instance = this;
        s_hasStartedInitialisation = false;
        
        s_OnSceneLoaded += InitialiseScene;
        s_OnSceneUnloaded += UnloadScene;
        SceneManager.sceneLoaded += s_OnSceneLoaded;
        SceneManager.sceneUnloaded += s_OnSceneUnloaded;
    }

    public void InitialiseBeforeStart()
    {
        InputSystem.onEvent += OnDeviceChange;
    }

    private void Initialise()
    {
        if (s_hasStartedInitialisation || s_hasInitialised) return;
        s_hasStartedInitialisation = true;
        _fadeManager.Show();
        _playerController.gameObject.SetActive(true);
        _playerController.HudController.gameObject.SetActive(true);
        _initialisation.gameObject.SetActive(true);
        s_initSceneName = _sceneHolder.GetSceneName(0);
        Debug.Log("Initialise method");
        if (!IsInitScene(SceneManager.GetActiveScene().name))
            SceneManager.LoadScene(s_initSceneName, LoadSceneMode.Single);
        else
            InitialiseScene(SceneManager.GetSceneAt(0), LoadSceneMode.Additive);
    }

    private void OnDestroy()
    {
        InputSystem.onEvent -= OnDeviceChange;
        if (Instance != this) return;
        s_OnSceneLoaded -= InitialiseScene;
        s_OnSceneUnloaded -= UnloadScene;
        SceneManager.sceneLoaded -= s_OnSceneLoaded;
        SceneManager.sceneUnloaded -= s_OnSceneUnloaded;
    }

    public void LoadNewScene(int idx)
    {
        if (_playerController.HasInitialised)
            _initialisation.TogglePlayer(false);
        if (!_fadeManager.IsOn)
            _fadeManager.StartFade(false, () => {LoadScene(idx);});
        else
            LoadScene(idx);
    }
    
    public void LoadNewScene(LoadZoneData data)
    {
        if (_playerController.HasInitialised)
            _initialisation.TogglePlayer(false);
        if (!_fadeManager.IsOn)
            _fadeManager.StartFade(false, () => {LoadScene(data);});
        else
            LoadScene(data);
    }

    private static void LoadScene(int idx)
    {
        s_currentZoneData = null;
        if (SceneManager.loadedSceneCount > 1)
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
        SceneManager.LoadScene(Instance._sceneHolder.GetSceneName(idx), LoadSceneMode.Additive);
        Debug.Log("Finished loading scene");
    }

    private static void LoadScene(LoadZoneData data)
    {
        s_currentZoneData = data;
        if (SceneManager.loadedSceneCount > 1)
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
        SceneManager.LoadScene(Instance._sceneHolder.GetSceneName(data.sceneIdx), LoadSceneMode.Additive);
        Debug.Log("Finished loading scene");
    }
    
    private void InitialiseScene(Scene loadedScene, LoadSceneMode loadSceneMode)
    {
        SceneManager.SetActiveScene(loadedScene);
        bool isFirstLoad = !s_hasInitialised;
        if (!IsInitScene(loadedScene.name) && !s_hasStartedInitialisation && !s_hasInitialised)
            s_hasInitialised = true;
        if (IsInitScene(loadedScene.name) || !s_hasInitialised || s_hasStartedInitialisation)
        {
            _fadeManager.StartFade(true);
            return;
        }
        
        EnemySpawner[] enemySpawners = FindObjectsByType<EnemySpawner>();
        foreach (EnemySpawner spawner in enemySpawners)
        {
            bool wasKilled = _saveManager.SaveData.GetEnemyIDKilled(spawner.EnemyID);
            if (!wasKilled)
                spawner.SpawnEnemy();
            else
                spawner.gameObject.SetActive(false);
        }
        
        if (s_currentZoneData != null)
            Instance._initialisation.MovePlayer(s_currentZoneData);
        else if (!isFirstLoad || !SaveManager.Instance.SaveData.doesDataExist)
        {
            PlayerSpawn[] spawns = FindObjectsByType<PlayerSpawn>();
            foreach (PlayerSpawn spawn in spawns)
            {
                if (!spawn.IsDefault) continue;
                _initialisation.MovePlayer(spawn._data);
                break;
            }
        }
        else if (SaveManager.Instance.SaveData.doesDataExist)
            _playerController.LoadSaveData();

        SaveManager.Instance.SaveData.currentSceneIdx = loadedScene.buildIndex;
        
        if (_playerController.HasInitialised)
            _initialisation.TogglePlayer(true);
        
        SaveManager.Instance.Save();
        _fadeManager.StartFade(true);
    }

    private void UnloadScene(Scene unloadedScene)
    {
        _initialisation.TogglePlayer(false);
    }

    private static bool IsInitScene(string sceneName) => sceneName == s_initSceneName;

    public void LoadSceneFromData(LoadZoneData data) => LoadNewScene(data);

    public void StartGame()
    {
        StartupCheck();
    }

    public void LoadMainMenuScene()
    {
        _playerController.gameObject.SetActive(false);
        _playerController.HudController.gameObject.SetActive(false);
        _initialisation.gameObject.SetActive(false);
        LoadNewScene(_sceneHolder.MainMenuSceneIdx);
    }

    private void OnDeviceChange(InputEventPtr eventPtr, InputDevice device)
    {
        if (s_lastDevice == device) return;
        if (eventPtr.type != StateEvent.Type) return;
        
        bool validPress = eventPtr.EnumerateChangedControls(device, 0.01F).Any();
        if (!validPress) return;
        
        switch (device)
        {
            case Keyboard:
            case Mouse:
            {
                if (ControlScheme == ControlScheme.KeyboardMouse) return;
                ControlScheme = ControlScheme.KeyboardMouse;
                OnControlSchemeChanged();
                break;
            }
            case Gamepad:
                if (ControlScheme == ControlScheme.Gamepad) return;
                ControlScheme = ControlScheme.Gamepad;
                OnControlSchemeChanged();
                break;
        }

        s_lastDevice = device;
    }

    private void OnControlSchemeChanged()
    {
        Debug.Log($"Changed control scheme to {ControlScheme}");
        
        _playerController.OnDeviceChange(ControlScheme);
    }
}