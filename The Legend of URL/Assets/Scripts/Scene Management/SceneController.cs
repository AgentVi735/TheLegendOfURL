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
        TogglePlayer(false);
        if (!fadeManager.IsOn)
            fadeManager.StartFade(false, () => {LoadScene(idx);});
        else
            LoadScene(idx);
    }

    private static void LoadScene(int idx)
    {
        if (!string.IsNullOrEmpty(currentSceneName))
            SceneManager.UnloadSceneAsync(currentSceneName);
        SceneManager.LoadScene(Instance.sceneHolder.GetSceneName(idx), LoadSceneMode.Additive);
        Debug.Log("Finished");
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
        TogglePlayer(true);
        fadeManager.StartFade(true);
    }

    private void UnloadScene(Scene unloadedScene)
    {
        TogglePlayer(false);
    }
    
    private void TogglePlayer(bool toggle)
    {
        playerController?.ToggleAllInputs(toggle);
    }

    private static bool IsInitScene(string sceneName) => sceneName == initSceneName;
}