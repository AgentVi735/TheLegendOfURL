#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SettingsUI _settingsUI;
    [SerializeField] private Camera _camera;

    [Header("UI")]
    [SerializeField] private UIButton _startButton;
    [SerializeField] private string _startText;
    [SerializeField] private string _continueText;
    [SerializeField] private UIButton _settingsButton;
    [SerializeField] private UIButton _quitButton;

    private void Awake()
    {
        InputSystem.EnableDevice(Mouse.current);
        InputSystem.EnableDevice(Keyboard.current);
        if (SaveManager.Instance == null)
            FindAnyObjectByType<SaveManager>().Initialise();
        _startButton.UpdateText(SaveManager.Instance.SaveData.doesDataExist ? _continueText : _startText);
        FindAnyObjectByType<SceneController>().InitialiseBeforeStart();
        _settingsUI.Initialise();
    }

    public void StartButton()
    {
        Destroy(_camera.gameObject);
        Destroy(gameObject);
        if (SceneController.Instance != null)
            FindAnyObjectByType<FadeManager>(FindObjectsInactive.Include).StartFade(false, SceneController.Instance.StartGame);
        else
        {
            SceneController sceneController = FindAnyObjectByType<SceneController>();
            FindAnyObjectByType<FadeManager>(FindObjectsInactive.Include).StartFade(false, sceneController.StartGame);
        }
    }

    public void SettingsButton()
    {
        gameObject.SetActive(false);
        _settingsUI.ToggleVisibility(true);
    }

    public void QuitButton()
    {
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
}