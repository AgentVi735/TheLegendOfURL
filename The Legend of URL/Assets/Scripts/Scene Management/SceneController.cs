using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;
    private static bool hasInitialised;
    private static bool hasStartedInitialisation;

    public static UnityAction<Scene, LoadSceneMode> onSceneLoaded;
    public static UnityAction<Scene> onSceneUnloaded;

    [SerializeField] private SceneHolder sceneHolder;
    [SerializeField] private GameInitialisation initialisation;
    [SerializeField] private FadeManager fadeManager;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SaveManager saveManager;

    private static string currentSceneName;
    private static string initSceneName;

    private static LoadZoneData currentZoneData;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        if (initialisation == null)
        {
            Initialise();
            if (gameObject != null)
                Destroy(gameObject);
            return;
        }
        
        if (!hasInitialised)
            Initialise();
        saveManager.Initialise();
        if (saveManager.SaveData.doesDataExist)
            playerController.LoadSaveData();
        Instance = this;
        hasStartedInitialisation = false;
        onSceneLoaded += InitialiseScene;
        onSceneUnloaded += UnloadScene;
        SceneManager.sceneLoaded += onSceneLoaded;
        SceneManager.sceneUnloaded += onSceneUnloaded;
    }

    private void Initialise()
    {
        if (hasStartedInitialisation || hasInitialised) return;
        hasStartedInitialisation = true;
        initSceneName = sceneHolder.GetSceneName(0);
        if (!IsInitScene(SceneManager.GetActiveScene().name))
            SceneManager.LoadScene(initSceneName, LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        onSceneLoaded -= InitialiseScene;
        onSceneUnloaded -= UnloadScene;
        SceneManager.sceneLoaded -= onSceneLoaded;
        SceneManager.sceneUnloaded -= onSceneUnloaded;
    }

    public void LoadNewScene(int idx)
    {
        if (playerController.HasInitialised)
            initialisation.TogglePlayer(false);
        if (!fadeManager.IsOn)
            fadeManager.StartFade(false, () => {LoadScene(idx);});
        else
            LoadScene(idx);
    }
    
    public void LoadNewScene(LoadZoneData data)
    {
        if (playerController.HasInitialised)
            initialisation.TogglePlayer(false);
        if (!fadeManager.IsOn)
            fadeManager.StartFade(false, () => {LoadScene(data);});
        else
            LoadScene(data);
    }

    private static void LoadScene(int idx)
    {
        currentZoneData = null;
        if (SceneManager.loadedSceneCount > 1)
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
        SceneManager.LoadScene(Instance.sceneHolder.GetSceneName(idx), LoadSceneMode.Additive);
        Debug.Log("Finished loading scene");
    }

    private static void LoadScene(LoadZoneData data)
    {
        currentZoneData = data;
        if (SceneManager.loadedSceneCount > 1)
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
        SceneManager.LoadScene(Instance.sceneHolder.GetSceneName(data.sceneIdx), LoadSceneMode.Additive);
        Debug.Log("Finished loading scene");
    }
    
    private void InitialiseScene(Scene loadedScene, LoadSceneMode loadSceneMode)
    {
        bool isFirstLoad = !hasInitialised;
        if (!IsInitScene(loadedScene.name) && !hasStartedInitialisation && !hasInitialised)
        {
            hasInitialised = true;
        }
        if (IsInitScene(loadedScene.name) || !hasInitialised || hasStartedInitialisation) return;
        EnemySpawner[] enemySpawners = FindObjectsByType<EnemySpawner>();
        foreach (EnemySpawner spawner in enemySpawners)
        {
            bool wasKilled = saveManager.SaveData.GetEnemyIDKilled(spawner.EnemyID);
            if (!wasKilled)
                spawner.SpawnEnemy();
            else
                spawner.gameObject.SetActive(false);
        }
        
        if (currentZoneData != null)
            Instance.initialisation.MovePlayer(currentZoneData);
        else if (!isFirstLoad || !SaveManager.Instance.SaveData.doesDataExist)
        {
            PlayerSpawn[] spawns = FindObjectsByType<PlayerSpawn>();
            foreach (PlayerSpawn spawn in spawns)
            {
                if (!spawn.IsDefault) continue;
                initialisation.MovePlayer(spawn._data);
                break;
            }
        }
        else if (SaveManager.Instance.SaveData.doesDataExist)
            playerController.LoadSaveData();

        SaveManager.Instance.SaveData.currentSceneIdx = loadedScene.buildIndex;
        
        if (playerController.HasInitialised)
            initialisation.TogglePlayer(true);
        
        SaveManager.Instance.Save();
        fadeManager.StartFade(true);
    }

    private void UnloadScene(Scene unloadedScene)
    {
        initialisation.TogglePlayer(false);
    }

    private static bool IsInitScene(string sceneName) => sceneName == initSceneName;

    public void LoadSceneFromData(LoadZoneData data) => LoadNewScene(data);
}