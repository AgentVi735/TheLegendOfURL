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

    private static string currentSceneName;
    private static string initSceneName;

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
        if (SceneManager.loadedSceneCount > 1)
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
        SceneManager.LoadScene(Instance.sceneHolder.GetSceneName(idx), LoadSceneMode.Additive);
        Debug.Log("Finished loading scene");
    }

    private static void LoadScene(LoadZoneData data)
    {
        if (SceneManager.loadedSceneCount > 1)
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
        Debug.Log(Instance.playerController.HasInitialised);
        // if (Instance.playerController.HasInitialised)
        //     Instance.initialisation.MovePlayer(data);
        SceneManager.LoadScene(Instance.sceneHolder.GetSceneName(data.sceneIdx), LoadSceneMode.Additive);
        Debug.Log("Finished loading scene");
    }
    
    private void InitialiseScene(Scene loadedScene, LoadSceneMode loadSceneMode)
    {
        Debug.Log($"{loadedScene.name} | {Instance?.gameObject.scene.name} | {hasStartedInitialisation} | {hasInitialised}");
        if (!IsInitScene(loadedScene.name) && !hasStartedInitialisation && !hasInitialised)
        {
            hasInitialised = true;
        }
        if (IsInitScene(loadedScene.name) || !hasInitialised || hasStartedInitialisation) return;
        EnemySpawner[] enemySpawners = FindObjectsByType<EnemySpawner>();
        foreach (EnemySpawner spawner in enemySpawners)
            spawner.SpawnEnemy();
        if (playerController.HasInitialised)
        {
            PlayerSpawn[] spawns = FindObjectsByType<PlayerSpawn>();
            Debug.Log(spawns.Length);
            foreach (PlayerSpawn spawn in spawns)
            {
                if (!spawn.IsDefault) continue;
                Debug.Log(spawn._data.newPosition);
                initialisation.MovePlayer(spawn._data);
                Debug.Log(playerController.transform.position);
                break;
            }
        }
        if (playerController.HasInitialised)
            initialisation.TogglePlayer(true);
        fadeManager.StartFade(true);
    }

    private void UnloadScene(Scene unloadedScene)
    {
        initialisation.TogglePlayer(false);
    }

    private static bool IsInitScene(string sceneName) => sceneName == initSceneName;

    public void LoadSceneFromData(LoadZoneData data) => LoadNewScene(data);
}