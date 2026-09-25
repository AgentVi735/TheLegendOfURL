using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISlider : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private TMP_Text _valueText;
    [SerializeField] private Image _image;
    [SerializeField] private SliderType _type;

    [SerializeField] private float _volumeMultiplier;

    public void SetValue(float value, float maxValue = 1)
    {
        _slider.maxValue = maxValue;
        if (_type is SliderType.SFX or SliderType.Music)
            value *= _volumeMultiplier;
        _slider.value = value;
    }

    public void OnValueChange()
    {
        float value = _slider.value;
        switch (_type)
        {
            case SliderType.SFX:
                SaveManager.Instance.SaveData.sfxVolume = value / _volumeMultiplier;
                break;
            case SliderType.Music:
                SaveManager.Instance.SaveData.musicVolume = value / _volumeMultiplier;
                break;
            case SliderType.MouseSensitivityX:
                SaveManager.Instance.SaveData.mouseSensitivity.x = value;
                break;
            case SliderType.MouseSensitivityY:
                SaveManager.Instance.SaveData.mouseSensitivity.y = value;
                break;
            case SliderType.ControllerSensitivityX:
                SaveManager.Instance.SaveData.controllerSensitivity.x = value;
                break;
            case SliderType.ControllerSensitivityY:
                SaveManager.Instance.SaveData.controllerSensitivity.y = value;
                break;
        }

        _valueText.text = $"{value}/{_slider.maxValue}";
    }
}