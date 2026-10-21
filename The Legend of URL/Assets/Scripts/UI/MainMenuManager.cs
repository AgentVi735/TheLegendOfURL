#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MainMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SettingsUI _settingsUI;
    [SerializeField] private GameObject _objectsToRemove;
    private GameObject lastSelectedObject;

    [Header("UI")]
    [SerializeField] private UIButton _startButton;
    [SerializeField] private string _startText;
    [SerializeField] private string _continueText;
    [SerializeField] private UIButton _settingsButton;
    [SerializeField] private UIButton _quitButton;
    [SerializeField] private UIButton _settingsReturnButton;

    private ControlScheme _currentControlScheme = ControlScheme.None;

    private void Awake()
    {
        lastSelectedObject = _startButton.gameObject;
        InputSystem.EnableDevice(Mouse.current);
        InputSystem.EnableDevice(Keyboard.current);
        if (SaveManager.Instance == null)
            FindAnyObjectByType<SaveManager>().Initialise();

        SaveManager.Instance.CanSave = true;
        _startButton.UpdateText(SaveManager.Instance.SaveData.doesDataExist ? _continueText : _startText);
        FindAnyObjectByType<SceneController>().InitialiseBeforeStart();
        _settingsUI.Initialise();
        _currentControlScheme = SceneController.ControlScheme;
        OnDeviceChanged(_currentControlScheme);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        _startButton.UpdateText(SaveManager.Instance.SaveData.doesDataExist ? _continueText : _startText);
        lastSelectedObject = _settingsButton.gameObject;
        SelectButton();
    }
    
    public void StartButton()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Destroy(_objectsToRemove);
        Destroy(_settingsUI.gameObject);
        Destroy(gameObject);
        SceneController.Instance.FadeManager.StartFade(false, SceneController.Instance.StartGame);
    }

    public void SettingsButton()
    {
        gameObject.SetActive(false);
        _settingsUI.ToggleVisibility(true);
        lastSelectedObject = _settingsReturnButton.gameObject;
        SelectButton();
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

    public void OnDeviceChanged(ControlScheme controlScheme)
    {
        if (_currentControlScheme == ControlScheme.None) return;
        _currentControlScheme = controlScheme;
        SelectButton();
    }

    private void SelectButton()
    {
        switch (_currentControlScheme)
        {
            default:
            case ControlScheme.None:
            case ControlScheme.Keyboard:
            case ControlScheme.Gamepad:
                if (lastSelectedObject == _settingsReturnButton.gameObject && !_settingsUI.gameObject.activeSelf)
                    lastSelectedObject = _settingsButton.gameObject;
                EventSystem.current.SetSelectedGameObject(lastSelectedObject);
                break;
            case ControlScheme.Mouse:
                lastSelectedObject = EventSystem.current.currentSelectedGameObject != null
                    ? EventSystem.current.currentSelectedGameObject
                    : _startButton.gameObject;
                EventSystem.current.SetSelectedGameObject(null);
                break;
        }
    }
}