#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SettingsUI _settingsUI;
    [SerializeField] private GameObject _objectsToRemove;

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

    public void Show()
    {
        _startButton.UpdateText(SaveManager.Instance.SaveData.doesDataExist ? _continueText : _startText);
        gameObject.SetActive(true);
    }
    
    public void StartButton()
    {
        Destroy(_objectsToRemove);
        Destroy(_settingsUI.gameObject);
        Destroy(gameObject);
        SceneController.Instance.FadeManager.StartFade(false, SceneController.Instance.StartGame);
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