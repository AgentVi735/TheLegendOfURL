using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MainMenuManager _mainMenu;
    
    [Header("UI")]
    [SerializeField] private UIButton _backButton;
    [SerializeField] private UISlider _sfxSlider;
    [SerializeField] private UISlider _musicSlider;
    [SerializeField] private UIFullscreenSwitch _fullscreenSwitch;
    [SerializeField] private UISlider _mouseSensitivityX;
    [SerializeField] private UISlider _mouseSensitivityY;
    [SerializeField] private UISlider _controllerSensitivityX;
    [SerializeField] private UISlider _controllerSensitivityY;

    [Header("Options")]
    [SerializeField] private float maxVolume = 100;
    [SerializeField] private float maxSensitivity = 200;

    public void Initialise()
    {
        ToggleVisibility(false);
        LoadSave();
    }

    private void LoadSave()
    {
        _sfxSlider.SetValue(SaveManager.Instance.SaveData.sfxVolume, maxVolume);
        _musicSlider.SetValue(SaveManager.Instance.SaveData.musicVolume, maxVolume);
        _fullscreenSwitch.SetCurrent(SaveManager.Instance.SaveData.fullscreenIdx);
        _mouseSensitivityX.SetValue(SaveManager.Instance.SaveData.mouseSensitivity.x, maxSensitivity);
        _mouseSensitivityY.SetValue(SaveManager.Instance.SaveData.mouseSensitivity.y, maxSensitivity);
        _controllerSensitivityX.SetValue(SaveManager.Instance.SaveData.controllerSensitivity.x, maxSensitivity);
        _controllerSensitivityY.SetValue(SaveManager.Instance.SaveData.controllerSensitivity.y, maxSensitivity);
    }
    
    public void ToggleVisibility(bool toggle) => gameObject.SetActive(toggle);

    public void BackButton()
    {
        gameObject.SetActive(false);
        SaveManager.Instance.Save();
        _mainMenu.gameObject.SetActive(true);
    }
}